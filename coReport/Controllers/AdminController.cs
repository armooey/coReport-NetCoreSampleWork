using coReport.Auth;
using coReport.Data;
using coReport.Models.AccountViewModels;
using coReport.Models.ActivityModels;
using coReport.Models.AdminViewModels;
using coReport.Models.HomeViewModels;
using coReport.Models.LogViewModel;
using coReport.Models.ProjectModels;
using coReport.Models.ProjectViewModels;
using coReport.Operations;
using coReport.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private IReportData _reportData;
        private IManagerReportData _managerReportData;
        private IWebHostEnvironment _webHostEnvironment;
        private RoleManager<IdentityRole<short>> _roleManager;
        private IManagerData _managerData;
        private IMessageService _messageService;
        private IProjectData _projectData;
        private ILogService _logger;
        private IActivityData _activityData;

        public AdminController(UserManager<ApplicationUser> userManager, IReportData reportData,
            IManagerReportData managerReportData,
            IWebHostEnvironment webHostEnvironment,
            RoleManager<IdentityRole<short>> roleManager,
            IManagerData managerData,
            IMessageService messageService,
            IProjectData projectService,
            ILogService logger,
            IActivityData activityData)
        {
            _userManager = userManager;
            _reportData = reportData;
            _managerReportData = managerReportData;
            _webHostEnvironment = webHostEnvironment;
            _roleManager = roleManager;
            _managerData = managerData;
            _messageService = messageService;
            _projectData = projectService;
            _logger = logger;
            _activityData = activityData;
        }

        public IActionResult AdminPanel()
        {
            //prepare datas for chart
            short ADMIN_ID = 1;
            var projects = _projectData.GetAll();
            var today = DateTime.Now;
            var userReports = _reportData.GetReportsOfLastSevenDays();
            var managerReports = _managerReportData.GetReportsOfLastSevenDays();
            var userReportCount = new List<int>();
            var managerReportCount = new List<int>();
            var projectViewModels = new List<ProjectViewModel>();
            var warnings = _messageService.GetWarnings();
            var warningViewModels = new List<WarningViewModel>();
            var activities = _activityData.GetAllActivities().ToList();
            
            //A list for the last 7 days
            var days = new List<String>();
            //counting number of reports in the last 7 days
            for (int i = -6; i <= 0; i++)
            {
                var day = today.AddDays(i); //finding the day that we want to count reports
                days.Add(day.ToHijri().GetDayAndMonth());//Adding to days List
                userReportCount.Add(userReports.Count(r => r.Date.Date == day.Date));
                managerReportCount.Add(managerReports.Count(r => r.Date.Date == day.Date));
            }


            foreach (var project in projects)
            {
                projectViewModels.Add(new ProjectViewModel { 
                    Id = project.Id,
                    Title = project.Title,
                    CreateDate = project.CreateDate.ToHijri().GetDate(),
                    EndDate = project.EndDate.HasValue ? project.EndDate.Value.ToHijri().GetDate() : null
                });
            }
            foreach (var warning in warnings)
            {
                warningViewModels.Add(new WarningViewModel { 
                    ReceiverName = String.Concat(warning.Receiver.FirstName," ",warning.Receiver.LastName),
                    Title = warning.Message.Title,
                    IsViewed = warning.IsViewd,
                    ElapsedTime = Math.Round(DateTime.Now.Subtract(warning.Message.Time).TotalHours)
                });
            }

            var adminPanelModel = new AdminPanelViewModel { 
                Days = days,
                Projects = projectViewModels,
                UsersReportCount = userReportCount,
                ManagersReportCount = managerReportCount,
                Warnings = warningViewModels,
                Messages = SystemOperations.GetMessageViewModels(_messageService, ADMIN_ID),
                Activities = activities
            };
            return View(adminPanelModel);
        }

        public async Task<IActionResult> ManageUsers()
        {
            var users = _userManager.Users.Where(user => user.UserName != "admin" && !user.IsDeleted).OrderByDescending(u => u.RegisterDate);
            var userViewModelList = new List<UserViewModel>();
            foreach (ApplicationUser user in users)
            {
                //creating view model
                var userRolesList = await _userManager.GetRolesAsync(user);
                var userRole = userRolesList.FirstOrDefault();
                userViewModelList.Add(new UserViewModel
                {
                    Id = user.Id,
                    Username = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    HasImage = user.ProfileImageName != null ? true : false,
                    Role = userRole,
                    RoleName = userRole == "Manager" ? AppSettingInMemoryDatabase.MANAGER_ROLE_NAME :
                                            AppSettingInMemoryDatabase.EMPLOYEE_ROLE_NAME,
                    IsActive = user.IsActive
                });
            }
            var userManagementViewModel = new UserManagementViewModel
            {
                Users = userViewModelList
            };
            return View(userManagementViewModel);
        }



        public async Task<IActionResult> ActivateUser(short userId, string userRole, UserManagementViewModel model = null)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            user.IsActive = true;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                if (model.ManagerIds != null)
                {
                    var setManagerResult = _managerData.SetManagers(userId, model.ManagerIds);
                    if (!setManagerResult)
                    {
                        return RedirectToAction("Error", "Home", new ErrorViewModel
                        {
                            Error = "مشکل در فعالسازی حساب کاربری!"
                        });
                    }
                }
                if (userRole == "Manager")
                    return Json(true);
                else
                    return RedirectToAction("ManageUsers");
            }
            if (userRole == "Manager")
                return Json(false);
            else
                return RedirectToAction("Error", "Home", new ErrorViewModel
                {
                    Error = "مشکل در فعالسازی حساب کاربری!"
                });
        }



        /*
         * Add user for admin
         * this action helps admin to add new user
         */
        [HttpGet]
        public IActionResult AddUser()
        {
            var model = new RegisterViewModel
            {
                Roles = _roleManager.GetRolesSelectList()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(RegisterViewModel model)
        {
            model.Roles = _roleManager.GetRolesSelectList();
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Username,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    RegisterDate = DateTime.Now,
                    IsActive = false,
                    LockoutEnabled = false,
                    Email = model.Email,
                    ProfileImageName = model.ImageName
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogProfileImageHistory(user.Id, model.ImageName);
                    await _userManager.AddToRoleAsync(user, model.Role);
                    return RedirectToAction("ManageUsers");
                }
                foreach (var error in result.Errors)
                {
                    switch (error.Code)
                    {
                        case "DuplicateUserName":
                            ModelState.AddModelError("", "این نام کاربری قبلا ثبت شده است.");
                            break;
                        case "DuplicateEmail":
                            ModelState.AddModelError("", "این ایمیل قبلا در سیستم ثبت شده است.");
                            break;
                        case "PasswordRequiresUpper":
                            ModelState.AddModelError("", "کلمه عبور باید شامل حروف بزرگ انگلیسی باشد.");
                            break;
                        case "PasswordRequiresDigit":
                            ModelState.AddModelError("", "کلمه عبور باید شامل اعداد باشد.");
                            break;
                        case "PasswordRequiresLower":
                            ModelState.AddModelError("", "کلمه عبور باید شامل حروف کوچک انگلیسی باشد.");
                            break;
                        default:
                            ModelState.AddModelError("", "مشکل در ثبت کاربر");
                            break;
                    }
                }
            }

            return View(model);
        }



        public async Task<IActionResult> DeleteUser(short userId)
        {
            var preprocessDeleteResult = _reportData.PreprocessUserDelete(userId);
            if (!preprocessDeleteResult)
                return Json(false);
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null)
            {
                user.IsDeleted = true;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                    return Json(true);
                else
                    return Json(false);
            }
            return Json(true);
        }

        public IActionResult AddProject(String projectName)
        {
            var result = _projectData.Add(new Project
            {
                Title = projectName,
                CreateDate = DateTime.Now
            });
            if (!result)
            {
                return RedirectToAction("Error","Home",new ErrorViewModel { 
                    Error = "مشکل در ایجاد پروژه جدید!"
                });
            }
            return RedirectToAction("AdminPanel");
        }

        public IActionResult AddActivity(String activityName, short? parentActivityName = null)
        {
            var result = _activityData.Add(new Activity { Name = activityName , ParentActivityId = parentActivityName});
            if (!result)
            {
                return RedirectToAction("Error", "Home", new ErrorViewModel
                {
                    Error = "مشکل در ایجاد فعالیت جدید!"
                });
            }
            return RedirectToAction("AdminPanel");
        }


        public IActionResult EndProject(short id)
        {
            var result = _projectData.EndProject(id);
            return Json(result);
        }

        public IActionResult DeleteActivity(short id)
        {
            var result = _activityData.Delete(id);
            return Json(result);
        }


        //getting server logs
        public IActionResult ServerLogs()
        {
            var logs = _logger.GetAll();
            var logViewModels = new List<LogViewModel>();
            foreach (var log in logs)
            {
                logViewModels.Add(new LogViewModel { 
                    Id = log.Id,
                    Message = log.Message,
                    Date = log.Date.ToHijri()
                });
            }
            return View(logViewModels);
        }

        public IActionResult GetException(short id)
        {
            return Json(_logger.GetLogMessage(id));
        }

        public IActionResult GetUserActivityDetails(int dayIndex, bool typeFlag)
        {
            var day = DateTime.Now.AddDays(dayIndex - 7);
            var detailsList = new List<JsonResult>();
            if (typeFlag) //if true means that user requested employee activity details
            {
                var employeeReports = _reportData.GetReportsOfDayIncludingAuthor(day);
                foreach (var employeeReport in employeeReports)
                {
                    var authorName = employeeReport.Author.FirstName + " " + employeeReport.Author.LastName;
                    var time = employeeReport.Date.ToHijri().GetTime();
                    detailsList.Add(Json(new List<String> { authorName, time }));
                }
            }
            else //means user requested manager activities
            {
                var managerReports = _managerReportData.GetReportsOfDayIncludingAuthor(day);
                foreach (var managerReport in managerReports)
                {
                    var authorName = managerReport.Author.FirstName + " " + managerReport.Author.LastName;
                    var time = managerReport.Date.ToHijri().GetTime();
                    detailsList.Add(Json(new List<String> { authorName, time }));
                }
            }
            return Json(detailsList);
        }
    }
}
