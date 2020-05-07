using coReport.Auth;
using coReport.Models;
using coReport.Models.HomeViewModels;
using coReport.Models.MessageModels;
using coReport.Models.MessageViewModels;
using coReport.Models.ReportModels;
using coReport.Models.ReportViewModel;
using coReport.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Controllers
{
    public class ManagerController: Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private IReportData _reportData;
        private IManagerReportData _managerReportData;

        public ManagerController(UserManager<ApplicationUser> userManager, IReportData reportData,
            IManagerReportData managerReportData,
            IWebHostEnvironment webHostEnvironment,
            RoleManager<IdentityRole<short>> roleManager,
            IManagerData managerData,
            IMessageService messageService)
        {
            _userManager = userManager;
            _reportData = reportData;
            _managerReportData = managerReportData;
        }

        public async Task<IActionResult> ManageReports(String nav)
        {
            var manager = await _userManager.FindByNameAsync(User.Identity.Name);
            var userReports = _reportData.GetAllReports(manager.Id);
            
            var userReportViewModels = new List<ReportViewModel>();

            foreach (var report in userReports)
            {
                userReportViewModels.Add(new ReportViewModel
                {
                    Id = report.Report.Id,
                    Title = report.Report.Title,
                    Author = report.Report.Author,
                    Text = report.Report.Text,
                    ProjectName = report.Report.Project.Title,
                    EnterTime = report.Report.EnterTime,
                    ExitTime = report.Report.ExitTime,
                    Date = report.Report.Date,
                    IsViewed = report.IsViewd
                });
            }
          
            var reportsViewModel = new UserAndManagerReportViewModel
            {
                UserReports = userReportViewModels
            };
            return View(reportsViewModel);
        }
    }
}
