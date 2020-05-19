using coReport.Auth;
using coReport.Data;
using coReport.Models.AccountViewModels;
using coReport.Models.AdminViewModels;
using coReport.Models.HomeViewModels;
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
        private IProjectData _projectService;
        private ILogService _logger;

        public AdminController(UserManager<ApplicationUser> userManager, IReportData reportData,
            IManagerReportData managerReportData,
            IWebHostEnvironment webHostEnvironment,
            RoleManager<IdentityRole<short>> roleManager,
            IManagerData managerData,
            IMessageService messageService,
            IProjectData projectService,
            ILogService logger)
        {
            _userManager = userManager;
            _reportData = reportData;
            _managerReportData = managerReportData;
            _webHostEnvironment = webHostEnvironment;
            _roleManager = roleManager;
            _managerData = managerData;
            _messageService = messageService;
            _projectService = projectService;
            _logger = logger;
        }

        public IActionResult AdminPanel()
        {
            //prepare datas for chart
            short ADMIN_ID = 1;
            var projects = _projectService.GetAll();
            var today = DateTime.Now;
            var userReports = _reportData.GetReportsOfLastSevenDays();
            var managerReports = _managerReportData.GetReportsOfLastSevenDays();
            var userReportCount = new List<int>();
            var managerReportCount = new List<int>();
            var projectViewModels = new List<ProjectViewModel>();
            var warnings = _messageService.GetWarnings();
            var warningViewModels = new List<WarningViewModel>();
            
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
                    CreateDate = project.CreateDate,
                    EndDate = project.EndDate
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
                Messages = SystemOperations.GetMessageViewModels(_messageService, ADMIN_ID)
            };
            return View(adminPanelModel);
        }

        public async Task<IActionResult> ManageUsers()
        {
            var users = _userManager.Users.Where(user => user.UserName != "admin" && !user.IsDeleted).OrderByDescending(u => u.RegisterDate);
            var userViewModelList = new List<UserViewModel>();
            foreach (ApplicationUser user in users)
            {
                //Reading image from local storage
                byte[] image;
                try
                {
                    var imagePath = Path.Combine(_webHostEnvironment.ContentRootPath, "UserData", "Images",
                        user.ProfileImageName + ".jpg");
                    image = await System.IO.File.ReadAllBytesAsync(imagePath);
                }
                catch
                {
                    image = null;
                }
                //creating view model
                var userRole = _userManager.GetRolesAsync(user).Result.FirstOrDefault();
                userViewModelList.Add(new UserViewModel
                {
                    Id = user.Id,
                    Username = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Image = image,
                    Role = userRole,
                    RoleName = userRole == "Manager"? AppSettingInMemoryDatabase.MANAGER_ROLE_NAME:
                                            AppSettingInMemoryDatabase.EMPLOYEE_ROLE_NAME,
                    IsActive = user.IsActive
                }) ;
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
                String imageName = null;
                if (model.Image != null)
                { 
                    imageName = await SystemOperations.SaveProfileImage(_webHostEnvironment, model.Image);
                    if (imageName == null)
                    {
                        ModelState.AddModelError("", "مشکل در ذخیره سازی عکس پروفایل");
                        return View(model);
                    }
                }
                var user = new ApplicationUser
                {
                    UserName = model.Username,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    RegisterDate = DateTime.Now,
                    IsActive = false,
                    LockoutEnabled = false,
                    Email = model.Email,
                    ProfileImageName = imageName
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogProfileImageHistory(user.Id, imageName);
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
            var result = _projectService.Add(new Project
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

        public IActionResult EndProject(short id)
        {
            var result = _projectService.EndProject(id);
            return Json(result);
        }
    }
}
