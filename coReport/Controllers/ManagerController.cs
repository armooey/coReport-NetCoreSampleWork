using coReport.Auth;
using coReport.Models;
using coReport.Models.HomeViewModels;
using coReport.Models.Message;
using coReport.Models.MessageViewModels;
using coReport.Models.Report;
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
        private IWebHostEnvironment _webHostEnvironment;
        private RoleManager<IdentityRole<short>> _roleManager;
        private IManagerData _managerData;
        private IMessageService _messageService;

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
            _webHostEnvironment = webHostEnvironment;
            _roleManager = roleManager;
            _managerData = managerData;
            _messageService = messageService;
        }

        public async Task<IActionResult> ManageReports(String nav)
        {
            var manager = await _userManager.FindByNameAsync(User.Identity.Name);
            var userReports = new List<Report>();
            //Getting report data based on navigation
            if (nav == "unseen")
                userReports = _reportData.GetUnseenReports(manager.Id).ToList();
            else if (nav == "today")
                userReports = _reportData.GetTodayReports(manager.Id).ToList();
            var userReportViewModels = new List<ReportViewModel>();

            foreach (var report in userReports)
            {
                userReportViewModels.Add(new ReportViewModel
                {
                    Id = report.Id,
                    Title = report.Title,
                    Author = report.Author,
                    Text = report.Text,
                    ProjectName = report.ProjectName,
                    EnterTime = report.EnterTime,
                    ExitTime = report.ExitTime,
                    Date = report.Date
                });
            }
          
            var managerReports = _managerReportData.GetAll(manager.Id);
            var managerReportsViewModel = new List<ManagerReportViewModel>();
            foreach (ManagerReport report in managerReports)
            {
                managerReportsViewModel.Add(new ManagerReportViewModel
                {
                    Id = report.Id,
                    Date = report.Date
                });
            }
            var reportsViewModel = new ReportsViewModel
            {
                UserReports = userReportViewModels,
                ManagerReports = managerReportsViewModel
            };
            ViewData["Navigation"] = nav;
            return View(reportsViewModel);
        }

        /*
         * View user reports
         */
        public async Task<IActionResult> ViewReport(short id)
        {
            var report = _reportData.Get(id);
            var manager = await _userManager.FindByNameAsync(User.Identity.Name);
            if (report != null)
            {
                _reportData.SetViewed(id, manager.Id);
                var reportModel = new ReportViewModel
                {
                    Id = report.Id,
                    Title = report.Title,
                    Author = report.Author,
                    ProjectName = report.ProjectName,
                    Text = report.Text,
                    EnterTime = report.EnterTime,
                    ExitTime = report.ExitTime,
                    Date = report.Date,
                    AttachmentName = report.AttachmentExtension != null ?
                            String.Format("{0}-{1}{2}", report.Author.UserName, report.Id, report.AttachmentExtension) : null
                };
                return View(reportModel);
            }
            return NotFound();
        }


        /*
         * Deleting the manager report
         */
        public IActionResult DeleteReport(short id)
        {
            try
            {
                _managerReportData.Delete(id);
            }
            catch (Exception e)
            {
                var errorModel = new ErrorViewModel
                {
                    Error = e.Message
                };
                return RedirectToAction("Error", "Home", errorModel);
            }
            return RedirectToAction("ManageReports", new { nav = "unseen" });
        }
    }
}
