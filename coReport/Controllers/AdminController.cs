using coReport.Auth;
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
    [Authorize(Roles = "ادمین")]
    public class AdminController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private IReportData _reportData;
        private IManagerReportData _managerReportData;
        private IWebHostEnvironment _webHostEnvironment;
        private RoleManager<IdentityRole<short>> _roleManager;
        private IManagerData _managerData;
        private IMessageService _messageService;
        private IProjectService _projectService;

        public AdminController(UserManager<ApplicationUser> userManager, IReportData reportData,
            IManagerReportData managerReportData,
            IWebHostEnvironment webHostEnvironment,
            RoleManager<IdentityRole<short>> roleManager,
            IManagerData managerData,
            IMessageService messageService,
            IProjectService projectService)
        {
            _userManager = userManager;
            _reportData = reportData;
            _managerReportData = managerReportData;
            _webHostEnvironment = webHostEnvironment;
            _roleManager = roleManager;
            _managerData = managerData;
            _messageService = messageService;
            _projectService = projectService;
        }

        public IActionResult AdminPanel()
        {
            //prepare datas for chart
            short ADMIN_ID = 1;
            var projects = _projectService.GetAll();
            var today = DateTime.Now.Date;
            var userReportCount = new List<int>();
            var managerReportCount = new List<int>();
            var projectViewModels = new List<ProjectViewModel>();
            var warnings = _messageService.GetWarnings();
            var warningViewModels = new List<WarningViewModel>();
            
            //creating list of the last 7 days
            var days = new List<String> { 
                today.AddDays(-6).ToString("MM/dd"),
                today.AddDays(-5).ToString("MM/dd"),
                today.AddDays(-4).ToString("MM/dd"),
                today.AddDays(-3).ToString("MM/dd"),
                today.AddDays(-2).ToString("MM/dd"),
                today.AddDays(-1).ToString("MM/dd"),
                today.ToString("MM/dd")
            };
            //counting number of reports in the last 7 days
            for (int i = -6; i <= 0; i++)
            {
                var day = today.AddDays(i); //finding the day that we want to count reports
                userReportCount.Add(_reportData.GetReportsCountByDate(day));
                managerReportCount.Add(_managerReportData.GetReportsCountByDate(day));
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
                    ElapsedTime = DateTime.Now.Subtract(warning.Message.Time).Hours
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

        public IActionResult ManageUsers()
        {
            var users = _userManager.Users.Where(user=>user.UserName != "admin").OrderByDescending(u => u.RegisterDate);
            var userViewModelList = new List<UserViewModel>();
            foreach (ApplicationUser user in users)
            {
                //Reading image from local storage
                byte[] image;
                try
                {
                    var imagePath = Path.Combine(_webHostEnvironment.ContentRootPath, "UserData", "Images", String.Format("{0}.jpg",user.UserName));
                    image = System.IO.File.ReadAllBytes(imagePath);
                }
                catch
                {
                    image = null;
                }
                //creating view model
                userViewModelList.Add(new UserViewModel
                {
                    Id = user.Id,
                    Username = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Image = image,
                    Role = _userManager.GetRolesAsync(user).Result.FirstOrDefault(),
                    IsActive = user.IsActive
                }) ;
            }
            var userManagementViewModel = new UserManagementViewModel
            {
                Users = userViewModelList
            };
            return View(userManagementViewModel);
        }



        public async Task<IActionResult> ActivateUser(short userId, UserManagementViewModel model = null)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            user.IsActive = true;
            var result = await _userManager.UpdateAsync(user);
            if(result.Succeeded)
            {
                if(model.ManagerIds != null)
                    _managerData.SetManagers(userId, model.ManagerIds);
                return RedirectToAction("ManageUsers");
            }
            return View("ManageUsers");
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
        public async Task<IActionResult> AddUser(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
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
                    Email = model.Email
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                    return RedirectToAction("ManageUsers");
                }
                ModelState.AddModelError("", "Cannot register this user.");
            }

            return View(model);
        }



        public async Task<IActionResult> DeleteUser(short userId)
        {
            _reportData.PreprocessUserDelete(userId);
            ApplicationUser user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null)
            {
                try
                {
                    IdentityResult result = await _userManager.DeleteAsync(user);
                    if (result.Succeeded)
                        return RedirectToAction("ManageUsers");
                    else
                        ModelState.AddModelError("", "Cannot delete this user!");
                }
                catch (Exception e)
                {
                    var errorModel = new ErrorViewModel
                    {
                        Error = e.Message
                    };
                    return RedirectToAction("Error", "Home", errorModel);
                }
            }
            else
            {
                ModelState.AddModelError("", "Cannot find user :/");
            }
            return RedirectToAction("ManageUsers");
        }

        public IActionResult AddProject(String projectName)
        {
            try
            {
                _projectService.Add(new Project
                {
                    Title = projectName,
                    CreateDate = DateTime.Now
                });
            }
            catch (Exception e)
            {
                 var errorModel = new ErrorViewModel
                    {
                        Error = e.Message
                    };
                    return RedirectToAction("Error", "Home", errorModel);
            }
            return RedirectToAction("AdminPanel");
        }

        public void EndProject(short id)
        {
            _projectService.EndProject(id);
        }
    }
}
