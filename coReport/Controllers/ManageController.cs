using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using coReport.Auth;
using coReport.Models.ManageViewModels;
using coReport.Operations;
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

        public ManageController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole<short>> roleManager,
        IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
        }



        /*
         * Index Action deals with user personal data like name.
         */
        [HttpGet]
        public async Task<IActionResult> Index(string userName, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["UserName"] = userName;
            var user = await _userManager.FindByNameAsync(userName);
            byte[] image;
            try
            {
                var imagePath = Path.Combine(_webHostEnvironment.ContentRootPath, "UserData", "Images", 
                    String.Format("{0}.jpg", user.ProfileImageName));
                image = await System.IO.File.ReadAllBytesAsync(imagePath);
            }
            catch
            {
                image = null;
            }

            var model = new IndexViewModel
            {
                Username = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                ImageByte = image
               
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(IndexViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            var user = await _userManager.FindByNameAsync(model.Username);
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            if (model.Image != null)
                user.ProfileImageName = await SystemOperations.SaveProfileImage(_webHostEnvironment, model.Image);
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            { 
                return RedirectToAction("Index",new {userName = model.Username, returnUrl = returnUrl });
            }
            ModelState.AddModelError(String.Empty, "مشکل در بروزرسانی اطلاعات کاربر.");
            return View(model);
        }



        /*
         * Used to change email
         */
        [HttpGet]
        public async Task<IActionResult> Email(string userName, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            var user = await _userManager.FindByNameAsync(userName);
            var model = new ChangeEmailViewModel
            {
                Email = user.Email
            };
            ViewData["UserName"] = userName;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Email(ChangeEmailViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["UserName"] = model.Username;
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null)
            {
                user.Email = model.Email;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                    return RedirectToAction("Index", new { userName = model.Username, returnUrl = returnUrl });
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
            var model = new AdministrationViewModel
            {
                Role = _userManager.GetRolesAsync(user).Result.FirstOrDefault(),
                Roles = _roleManager.GetRolesSelectList(),
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
            model.Roles = _roleManager.GetRolesSelectList();
            if (model.IsBanned && model.BanEnd < DateTime.Now)
            {
                ModelState.AddModelError(String.Empty, "زمان وارد شده گذشته است.");
                return View(model);
            }
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null)
            {
                user.IsBanned = model.IsBanned;
                user.BanEndTime = model.BanEnd;
                //removing previous role and adding new one
                var role = _userManager.GetRolesAsync(user).Result.FirstOrDefault();
                if(role != null)
                    await _userManager.RemoveFromRoleAsync(user, role);
                await _userManager.AddToRoleAsync(user, model.Role);
                //applying user update
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                    return RedirectToAction("Index", new { userName = model.Username, returnUrl = returnUrl });
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
                    return RedirectToAction(nameof(Index), new {userName =  model.Username, returnUrl = returnUrl });
                }
                ModelState.AddModelError(String.Empty, "مشکل در بروزرسانی اطلاعات کاربر.");
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
