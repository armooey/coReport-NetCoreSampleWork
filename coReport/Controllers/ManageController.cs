using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using coReport.Auth;
using coReport.Models.AccountViewModels;
using coReport.Models.ManageViewModels;
using coReport.Operations;
using coReport.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace coReport.Controllers
{

    [Authorize]
    public class ManageController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole<short>> _roleManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IManagerData _managerData;
        private readonly ILogService _logger;

        public ManageController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole<short>> roleManager,
        IWebHostEnvironment webHostEnvironment,
        IManagerData managerData,
        ILogService logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
            _managerData = managerData;
            _logger = logger;
        }



        /*
         * This Action deals with user personal data like name, phone, ...
         */
        [HttpGet]
        public async Task<IActionResult> AccountInformation(string userName, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["UserName"] = userName;
            if(!User.IsInRole("Admin") && User.Identity.Name != userName)
                return RedirectToAction("AccessDenied", "Home");
            var user = await _userManager.FindByNameAsync(userName);

            var model = new AccountInfoViewModel
            {
                Username = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                HasImage = user.ProfileImageName != null ? true : false
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AccountInformation(AccountInfoViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["UserName"] = model.Username;

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.Email = model.Email;
                if (model.Image != null)
                {
                    var imageName = await SystemOperations.SaveProfileImage(_webHostEnvironment, model.Image);
                    if (imageName == null)
                    {
                        ModelState.AddModelError("", "مشکل در ذخیره سازی عکس پروفایل");
                        return View(model);
                    }
                    _logger.LogProfileImageHistory(user.Id, imageName);
                    user.ProfileImageName = imageName;
                }
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("AccountInformation", new { userName = model.Username, returnUrl = returnUrl });
                }
                ModelState.AddModelError(String.Empty, "مشکل در بروزرسانی اطلاعات کاربر.");
                return View(model);
            }
            return View(model);

        }


        /*
         * Admin Operations like baning user
         */
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Administration(string userName, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["UserName"] = userName;
            var user = await _userManager.FindByNameAsync(userName);
            var managers = _managerData.GetManagers(user.Id).Select(m => m.Id).ToList();
            var allManagers = await _userManager.GetUsersInRoleAsync("Manager");
            allManagers = allManagers.Where(m => !m.IsDeleted).ToList();

            var managerViewModels = new List<UserViewModel>();
            foreach (var manager in allManagers.Where(m => m.Id != user.Id).ToList())
            {
                managerViewModels.Add(new UserViewModel { 
                    Id = manager.Id,
                    FirstName = manager.FirstName,
                    LastName = manager.LastName
                });
            }
            var model = new AdministrationViewModel
            {
                Role = _userManager.GetRolesAsync(user).Result.FirstOrDefault(),
                Roles = _roleManager.GetRolesSelectList(),
                Managers = managerViewModels,
                ManagerIds = managers,
                IsBanned = user.IsBanned,
                BanEnd = user.BanEndTime 
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Administration(AdministrationViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["UserName"] = model.Username;

            var user = await _userManager.FindByNameAsync(model.Username);
            var allManagers = await _userManager.GetUsersInRoleAsync("Manager");
            allManagers = allManagers.Where(m => !m.IsDeleted).ToList();
            var managers = _managerData.GetManagers(user.Id).Select(m => m.Id).ToList();
            var managerViewModels = new List<UserViewModel>();
            model.Managers = managerViewModels;
            model.Roles = _roleManager.GetRolesSelectList();
            model.ManagerIds = managers;
            if (model.IsBanned && model.BanEnd < DateTime.Now)
            {
                ModelState.AddModelError(String.Empty, "زمان وارد شده گذشته است.");
                return View(model);
            }

            foreach (var manager in allManagers.Where(m => m.Id != user.Id).ToList())
            {
                managerViewModels.Add(new UserViewModel
                {
                    Id = manager.Id,
                    FirstName = manager.FirstName,
                    LastName = manager.LastName
                });
            }

            if (user != null)
            {
                user.IsBanned = model.IsBanned;
                user.BanEndTime = model.BanEnd;
                //removing previous role and adding new one
                var role = _userManager.GetRolesAsync(user).Result.FirstOrDefault();
                if(role != null)
                    await _userManager.RemoveFromRoleAsync(user, role);
                if (model.Role != "Employee")
                    _managerData.DeleteManagers(user.Id);
                else if (model.Role == "Employee")
                {
                    _managerData.UpdateManagers(user.Id, model.ManagerIds);
                }
                await _userManager.AddToRoleAsync(user, model.Role);
                //applying user update
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                    return RedirectToAction("AccountInformation", new { userName = model.Username, returnUrl = returnUrl });
                ModelState.AddModelError(String.Empty, "مشکل در بروزرسانی اطلاعات کاربر.");
                return View(model);
            }
            return View(model);
        }

        /*
         * Changing password
         */
        [HttpGet]
        public IActionResult Password(String userName, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["UserName"] = userName;
            if(!User.IsInRole("Admin") && User.Identity.Name != userName)
                return RedirectToAction("AccessDenied", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Password(ChangePasswordViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["UserName"] = model.Username;
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null)
            {
                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.Password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("AccountInformation", new {userName =  model.Username, returnUrl = returnUrl });
                }
                foreach (var error in result.Errors)
                {
                    switch (error.Code)
                    {
                        case "PasswordRequiresUpper":
                            ModelState.AddModelError("", "کلمه عبور باید شامل حروف بزرگ انگلیسی باشد.");
                            break;
                        case "PasswordMismatch":
                            ModelState.AddModelError("", "کلمه عبور فعلی صحیح نیست.");
                            break;
                        case "PasswordRequiresDigit":
                            ModelState.AddModelError("", "کلمه عبور باید شامل اعداد باشد.");
                            break;
                        case "PasswordRequiresLower":
                            ModelState.AddModelError("", "کلمه عبور باید شامل حروف کوچک انگلیسی باشد.");
                            break;
                        default:
                            ModelState.AddModelError("", "مشکل در بروزرسانی اطلاعات کاربر");
                            break;
                    }
                }
                return View(model);
            }
            return View(model);
        }

        /*
         * This action redirects the user to page that the user came from.
         */

        public IActionResult Return(string returnUrl)
        {
            return Redirect(returnUrl);
        }

    }
}
