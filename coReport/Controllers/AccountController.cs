using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using coReport.Auth;
using coReport.Models.AccountViewModels;
using coReport.Models.Message;
using coReport.Models.MessageViewModels;
using coReport.Models.Operations;
using coReport.Models.ReportViewModel;
using coReport.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace coReport.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole<short>> _roleManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IReportData _reportData;
        private readonly IMessageService _messageService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole<short>> roleManager,
            IWebHostEnvironment webHostEnvironment,
            IReportData reportData,
            IMessageService messageService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
            _reportData = reportData;
            _messageService = messageService;
        }



        //Login Action
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Username,
                    model.Password, model.RememberMe,
                    lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(model.Username);
                    if (user.IsBanned && user.BanEndTime > DateTime.Now) //If user is banned redirect to ban message
                    {
                        await _signInManager.SignOutAsync();
                        return View("Banned");
                    }
                    else if (!user.IsActive) //If user is not activated redirect to inactive message
                    {
                        await _signInManager.SignOutAsync();
                        return View("Inactive");
                    }
                    else
                    {
                        return RedirectToLocal(returnUrl);
                    }

                }
                ModelState.AddModelError(string.Empty, "نام کاربری/رمز عبور اشتباه است.");
                return View(model);
            }


            return View(model);
        }



        //Register Action
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            var model = new RegisterViewModel
            {
                Roles = _roleManager.GetRolesSelectList()
            };
            return View(model);
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            model.Roles = _roleManager.GetRolesSelectList();
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { 
                    UserName = model.Username,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    RegisterDate = DateTime.Now,
                    IsActive = false,
                    IsBanned = false,
                    Email = model.Email };


                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                    UserOperations.SaveProfileImage(_webHostEnvironment, model.Image, model.Username);
                    var message = new Message {
                        Text = String.Format("{0} {1} با نام کاربری {2} حساب کاربری جدیدی ایجاد کرده است. لطفا نسبت به فعالسازی آن اقدام فرمایید.", 
                               model.FirstName, model.LastName, model.Username),
                        Type = MessageType.System_Notification,
                        Title = "کاربر جدید",
                        Time = DateTime.Now,
                        SenderId = user.Id
                    };
                    _messageService.AddSystemNotificationForAdmin(message);
                    return View("Inactive");
                }
                ModelState.AddModelError(String.Empty,"مشکل در ساخت حساب کاربری.");
            }


            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }


        public async Task<IActionResult> ManageReports()
        {

            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var reports = _reportData.GetByAuthorId(user.Id);
            var userMessages = _messageService.GetReceivedMessages(user.Id);
            var reportViewModels = new List<ReportViewModel>();
            var messageViewModels = new List<MessageViewModel>();
            foreach(var report in reports)
            {
                reportViewModels.Add(new ReportViewModel { 
                    Id = report.Id,
                    Title = report.Title,
                    ProjectName = report.ProjectName
                });
            }
            foreach (var userMessage in userMessages)
            {
                messageViewModels.Add(new MessageViewModel { 
                    Id = userMessage.Message.Id,
                    Title = userMessage.Message.Title,
                    Text = userMessage.Message.Text,
                    AuthorName = String.Concat(userMessage.Message.Sender.FirstName," ", userMessage.Message.Sender.LastName),
                    Type = userMessage.Message.Type,
                    IsViewed = userMessage.IsViewd
                });
            }
            var userReportViewModel = new UserReportViewModel { 
                Reports = reportViewModels,
                Messages = messageViewModels
            };
            return View(userReportViewModel);
        }


        /*
         * Used to get current user profile image. Used in Layout.
         */
        public IActionResult GetUserImage() 
        {
            var username = User.Identity.Name;
            byte[] image;
            try
            {
                var imagePath = string.Format("{0}\\UserData\\Images\\{1}.jpg", _webHostEnvironment.ContentRootPath, username);
                image = System.IO.File.ReadAllBytes(imagePath);
            }
            catch
            {
                var imagePath = string.Format("{0}\\Images\\user.png", _webHostEnvironment.WebRootPath);
                image = System.IO.File.ReadAllBytes(imagePath);
            }
            return File(image, "image/jpeg");
        }


        #region Helpers

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        #endregion
    }
}
