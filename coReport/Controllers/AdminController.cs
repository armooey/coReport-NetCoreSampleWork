using coReport.Auth;
using coReport.Models;
using coReport.Models.AccountViewModels;
using coReport.Models.AdminViewModels;
using coReport.Models.HomeViewModels;
using coReport.Models.Message;
using coReport.Models.MessageViewModels;
using coReport.Models.Operations;
using coReport.Models.Report;
using coReport.Models.ReportViewModel;
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

        public AdminController(UserManager<ApplicationUser> userManager, IReportData reportData,
            IManagerReportData managerReportData,
            IWebHostEnvironment webHostEnvironment,
            RoleManager<IdentityRole<short>> roleManager,
            IManagerData managerData,
            IMessageService messageService)
        {
            _userManager = userManager;
            _reportData = reportData;
            _managerReportData = managerReportData;
            _webHostEnvironment = webHostEnvironment;
            _roleManager = roleManager;
            _managerData = managerData;
            _messageService = messageService;
        }

        public IActionResult AdminPanel()
        {
            _messageService.CreateSystemNotifications();
            //prepare datas for chart
            short ADMIN_ID = 1;
            var userMessages = _messageService.GetReceivedMessages(ADMIN_ID);
            var today = DateTime.Now.Date;
            var userReportCount = new List<int>();
            var managerReportCount = new List<int>();
            var messageViewModels = new List<MessageViewModel>();

            //creating a list of the last 7 days
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
            foreach (var userMessage in userMessages)
            {
                messageViewModels.Add(new MessageViewModel
                {
                    Id = userMessage.Message.Id,
                    Title = userMessage.Message.Title,
                    Text = userMessage.Message.Text,
                    AuthorName = userMessage.Message.Type == MessageType.System_Notification ? "پیام سیستمی" :
                                String.Concat(userMessage.Message.Sender.FirstName, " ", userMessage.Message.Sender.LastName),
                    Type = userMessage.Message.Type,
                    IsViewed = userMessage.IsViewd
                });
            }
            var adminPanelModel = new AdminPanelViewModel { 
                Days = days,
                UsersReportCount = userReportCount,
                ManagersReportCount = managerReportCount,
                Messages = messageViewModels
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
    }
}
