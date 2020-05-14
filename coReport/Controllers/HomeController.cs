using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using coReport.Services;
using Microsoft.AspNetCore.Identity;
using coReport.Auth;
using coReport.Models.HomeViewModels;

namespace coReport.Controllers
{
    public class HomeController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;
        private IMessageService _messageService;

        public HomeController( IMessageService messageService,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _messageService = messageService;
        }

        public async Task<IActionResult> Index()
        {
            if(_signInManager.IsSignedIn(User))
            {
                var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
                var userRole = await _userManager.GetRolesAsync(currentUser);
                var alertsCount = _messageService.GetWarningsCount(currentUser.Id);
                var model = new HomeViewModel
                {
                    AlertsCount = alertsCount,
                    Name = userRole.FirstOrDefault() != "ادمین" ?
                                                currentUser.FirstName +  " " + currentUser.LastName : "ادمین",
                    Role = userRole.FirstOrDefault()
                };
                return View(model);
            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(ErrorViewModel model)
        {
            return View(model);
        }
    }
}
