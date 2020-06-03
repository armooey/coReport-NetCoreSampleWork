using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using coReport.Auth;
using coReport.Models.AccountViewModels;
using coReport.Models.MessageModels;
using coReport.Models.ReportViewModel;
using coReport.Operations;
using coReport.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
        private readonly ILogService _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole<short>> roleManager,
            IWebHostEnvironment webHostEnvironment,
            IReportData reportData,
            IMessageService messageService,
            ILogService logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
            _reportData = reportData;
            _messageService = messageService;
            _logger = logger;
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
                var user = new ApplicationUser {
                    UserName = model.Username,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    RegisterDate = DateTime.Now,
                    IsActive = false,
                    IsBanned = false,
                    Email = model.Email,
                    ProfileImageName = imageName
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogProfileImageHistory(user.Id, imageName);
                    await _userManager.AddToRoleAsync(user, model.Role);
                    var message = new Message {
                        //Quill.js text data
                        Text = "{\"ops\":[{\"attributes\":{\"font\":\"default\",\"size\":\"15px\",\"bold\":true},\"insert\":\""+
                        user.FirstName+ " " + user.LastName +" \"}," +
                        "{\"attributes\":{\"font\":\"default\",\"size\":\"15px\"},\"insert\":\" با نام کاربری \"}," +
                        "{\"attributes\":{\"font\":\"default\",\"size\":\"15px\",\"bold\":true},\"insert\":\" " +user.UserName +" \"}," +
                        "{\"attributes\":{\"font\":\"default\",\"size\":\"15px\"},\"insert\":\"حساب کاربری جدیدی ایجاد کرده است.\"}," +
                        "{\"attributes\":{\"align\":\"right\",\"direction\":\"rtl\"},\"insert\":\"\\n\"}," +
                        "{\"attributes\":{\"font\":\"default\",\"size\":\"15px\"},\"insert\":\"لطفا هر چه سریعتر نسبت به فعالسازی آن اقدام فرمایید.\"}," +
                        "{\"attributes\":{\"align\":\"right\",\"direction\":\"rtl\"},\"insert\":\"\\n\"}]}",
                        Type = MessageType.System_Notification,
                        Title = "کاربر جدید",
                        Time = DateTime.Now,
                        SenderId = user.Id
                    };
                    _messageService.AddSystemNotificationForAdmin(message);
                    return View("Inactive");
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

        //A remote validation to check if username is exits or not
        [AllowAnonymous]
        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> CheckUsernameExistance(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return Json(true);
            return Json("این نام کاربری قبلا در سیستم ثبت شده است.");
        }

        //A remote validation to check if email is exits or not
        [AllowAnonymous]
        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> CheckEmailExistance(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Json(true);
            return Json("این ایمیل قبلا در سیستم ثبت شده است.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> ManageReports()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var reports = _reportData.GetByAuthorId(user.Id);
            var reportViewModels = new List<ReportViewModel>();
            foreach (var report in reports)
            {
                reportViewModels.Add(new ReportViewModel {
                    Id = report.Id,
                    ProjectName = report.Project.Title,
                    Date = report.Date.ToHijri(),
                    Title = report.Title,
                    IsViewed = report.ProjectManagers.Any(pm => pm.IsViewd == true)
                });
            }

            var userReportViewModel = new UserReportViewModel {
                Reports = reportViewModels,
                Messages = SystemOperations.GetMessageViewModels(_messageService, user.Id)
            };
            return View(userReportViewModel);
        }


        /*
         * Used for getting current user profile image in Layout.
         */
        public async Task<IActionResult> GetUserImage() 
        {
            var user = await _userManager.GetUserAsync(User);
            byte[] image;
            try
            {
                var imagePath = Path.Combine(_webHostEnvironment.ContentRootPath, "UserData", "Images",
                    user.ProfileImageName +".jpg");
                image = await System.IO.File.ReadAllBytesAsync(imagePath);
            }
            catch
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "user.png");
                image = await System.IO.File.ReadAllBytesAsync(imagePath);
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
