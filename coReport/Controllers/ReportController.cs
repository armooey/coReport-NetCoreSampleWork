using coReport.Auth;
using coReport.Models.AccountViewModels;
using coReport.Models.HomeViewModels;
using coReport.Models.MessageModels;
using coReport.Models.ProjectViewModels;
using coReport.Models.ReportModels;
using coReport.Models.ReportViewModel;
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
    [Authorize]
    public class ReportController : Controller
    {
        private IReportData _reportData;
        private UserManager<ApplicationUser> _userManager;
        private IManagerReportData _managerReportData;
        private IWebHostEnvironment _webHostEnvironment;
        private IManagerData _managerData;
        private IMessageService _messageService;
        private IProjectService _projectService;

        public ReportController(IReportData reportData, IManagerReportData adminReportData,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment,
            IManagerData managerData,
            IMessageService messageService,
            IProjectService projectService)
        {
            _reportData = reportData;
            _userManager = userManager;
            _managerReportData = adminReportData;
            _webHostEnvironment = webHostEnvironment;
            _managerData = managerData;
            _messageService = messageService;
            _projectService = projectService;
        }


        /*
         * Creating user report
         */

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var author = await _userManager.FindByNameAsync(User.Identity.Name);
            var model = new CreateReportViewModel
            {
                AuthorId = author.Id,
                Managers = UserOperations.GetProjectManagerViewModels(author.Id, _managerData) ,//List of all managers of this user
                Projects = UserOperations.GetInProgressProjectViewModels(_projectService) //All in progress projects
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateReportViewModel model)
        {
            model.Managers = UserOperations.GetProjectManagerViewModels(model.AuthorId, _managerData);
            model.Projects = UserOperations.GetInProgressProjectViewModels(_projectService);
            if (ModelState.IsValid)
            {
                var todayReports = _reportData.GetTodayReportsOfUser(model.AuthorId);
                if (todayReports.Any(r => r.ProjectId == model.ProjectId))
                {
                    ModelState.AddModelError("", "امکان ثبت گزارش به دلیل وجود گزارشی به تاریخ امروز برای این پروژه وجود ندارد.");
                    return View(model);
                }
                if (model.EnterTime >= model.ExitTime)
                {
                    ModelState.AddModelError("", "زمان ورود و خروج را بررسی کنید.");
                    return View(model);
                }
                var report = new Report
                {
                    Title = model.Title,
                    Text = model.Text,
                    ProjectId = model.ProjectId,
                    AuthorId = model.AuthorId,
                    EnterTime = model.EnterTime,
                    ExitTime = model.ExitTime,
                    Date = DateTime.Now
                };
                try
                {
                    var savedReport = _reportData.Add(report, model.ProjectManagerIds); //Saving Report
                    //Save report Attachment
                    if (model.Attachment != null)
                    {
                        UserOperations.SaveReportAttachment(_webHostEnvironment, model.Attachment, model.AuthorId, savedReport.Id);
                        _reportData.UpdateAttachment(savedReport.Id, Path.GetExtension(model.Attachment.FileName));
                    }
                }
                catch (Exception e)
                {
                    var errorModel = new ErrorViewModel {
                        Error = e.Message
                    };
                    return RedirectToAction("Error","Home",errorModel);
                }
                return RedirectToAction("ManageReports","Account");
            }
            return View(model);
        }


        /*
         * Edit user report
         */
        [HttpGet]
        public IActionResult Edit(short id)
        {
            var report = _reportData.Get(id);
            
            var model = new CreateReportViewModel
            {
                Id = report.Id,
                AuthorId = report.AuthorId,
                EnterTime = report.EnterTime,
                ExitTime = report.ExitTime,
                ProjectId = report.ProjectId,
                Managers = UserOperations.GetProjectManagerViewModels(report.AuthorId, _managerData),
                Projects = UserOperations.GetInProgressProjectViewModels(_projectService),
                ProjectManagerIds = report.ProjectManagers.Select(pm => pm.ManagerId).ToList(),
                Title = report.Title,
                Text = report.Text,
                IsSubmitedByManager = report.ManagerReports != null && report.ManagerReports.Any() ? true : false, //If any of project managers submited report with this report
                AttachmentName = report.AttachmentExtension != null ? 
                                String.Format("{0}-{1}{2}", report.AuthorId, report.Id, report.AttachmentExtension): null
            };
           

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CreateReportViewModel model)
        {
            model.Managers = UserOperations.GetProjectManagerViewModels(model.AuthorId, _managerData);
            model.Projects = UserOperations.GetInProgressProjectViewModels(_projectService);
            if (ModelState.IsValid)
            {
                if (model.EnterTime >= model.ExitTime)
                {
                    ModelState.AddModelError("", "زمان ورود و خروج را بررسی کنید.");
                    return View(model);
                }
                var report = _reportData.Get(model.Id);
                report.Title = model.Title;
                report.Text = model.Text;
                report.ProjectId = model.ProjectId;
                report.EnterTime = model.EnterTime;
                report.ExitTime = model.ExitTime;
                report.Date = DateTime.Now;

                try
                {
                    _reportData.Update(report, model.ProjectManagerIds);
                    //update attachment if user provided new one
                    if (model.Attachment != null)
                    {
                        UserOperations.SaveReportAttachment(_webHostEnvironment, model.Attachment, model.AuthorId, report.Id, report.AttachmentExtension);
                        _reportData.UpdateAttachment(report.Id, Path.GetExtension(model.Attachment.FileName));
                    }
                }
                catch (Exception e)
                {
                    var errorModel = new ErrorViewModel
                    {
                        Error = e.Message
                    };
                    return RedirectToAction("Error","Home",errorModel);
                }
                return RedirectToAction("ManageReports","Account");
            }
            return View(model);
        }

        /*
         * Deleting user report
         */
        public IActionResult Delete(short id)
        {
            try
            {
                _reportData.Delete(id);
            }
            catch (Exception e)
            {
                var errorModel = new ErrorViewModel
                {
                    Error = e.Message
                };
                return RedirectToAction("Error","Home",errorModel);
            }
            return RedirectToAction("ManageReports","Account");
        }

   
        /*
         * View user reports
         */
        [Authorize(Roles = "مدیر")]
        public async Task<IActionResult> CheckUserReport(short id)
        {
            var report = _reportData.Get(id);
            var manager = await _userManager.FindByNameAsync(User.Identity.Name);
            var managerReport = _managerReportData.GetManagerReportByUserReportId(id, manager.Id);
            if (report != null)
            {
                var reportModel = new ReportViewModel
                {
                    Id = report.Id,
                    Title = report.Title,
                    Author = report.Author,
                    ProjectName = report.Project.Title,
                    Text = report.Text,
                    EnterTime = report.EnterTime,
                    ExitTime = report.ExitTime,
                    Date = report.Date,
                    AttachmentName = report.AttachmentExtension != null ?
                            String.Format("{0}-{1}{2}", report.AuthorId, report.Id, report.AttachmentExtension) : null
                };
                var managerReportViewModel = new ManagerReportViewModel { UserReport = reportModel };
                if (managerReport != null)
                {
                    managerReportViewModel.Id = managerReport.Id;
                    managerReportViewModel.Text = managerReport.Text;
                    managerReportViewModel.IsAcceptable = managerReport.IsUserReportAcceptable;
                    managerReportViewModel.IsViewableByUser = managerReport.IsCommentViewableByUser;
                }
                return View(managerReportViewModel);
            }
            return NotFound();
        }



        [Authorize(Roles = "مدیر")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateManagerReport(ManagerReportViewModel model)
        {
            var manager = await _userManager.FindByNameAsync(User.Identity.Name);
            var managerReport = _managerReportData.GetManagerReportByUserReportId(model.UserReport.Id, manager.Id);
            ManagerReport savedManagerReport;
            if (ModelState.IsValid)
            {
                if (managerReport == null) //means that user creating a new manager report
                {
                    managerReport = new ManagerReport
                    {
                        Date = DateTime.Now.Date,
                        Author = manager,
                        Text = model.Text,
                        IsUserReportAcceptable = model.IsAcceptable,
                        IsCommentViewableByUser = model.IsViewableByUser,
                        ReportId = model.UserReport.Id
                    };
                    try
                    {
                        savedManagerReport = _managerReportData.Add(managerReport);
                        //Change the report status to viewd
                        _reportData.SetViewed(model.UserReport.Id, manager.Id);
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
                else //means user updating manager report
                {
                    savedManagerReport = managerReport;
                    managerReport.Text = model.Text;
                    managerReport.IsUserReportAcceptable = model.IsAcceptable;
                    managerReport.IsCommentViewableByUser = model.IsViewableByUser;
                    try
                    {
                        _managerReportData.Update(managerReport);
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
                try 
                {

                    //Notify user if manager report is viewable by user
                    if (model.IsViewableByUser)
                    {
                        var message = new Message
                        {
                            Title = "گزارش مدیر",
                            Text = model.Text,
                            Sender = manager,
                            Type = MessageType.Manager_Report_Notification,
                            Time = DateTime.Now,
                            HelperId = String.Concat(savedManagerReport.Id, "-", model.UserReport.Id)
                        };
                        _messageService.AddManagerReviewMessage(message, model.UserReport.Author.Id);
                    }
                    else
                    {
                        _messageService.DeleteManagerReviewMessage(String.Concat(savedManagerReport.Id, "-", model.UserReport.Id));
                    }
                }
                catch (Exception e)
                {
                    var errorModel = new ErrorViewModel
                    {
                        Error = e.Message
                    };
                    return RedirectToAction("Error", "Home", errorModel);
                }
                return RedirectToAction("ManageReports", "Manager");
            }
            return View(model);
        }


        [HttpGet("download")]
        public ActionResult DownloadReportAttachment(String fileName)
        {
            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "UserData", "Files", fileName);
            try
            {
                byte[] file = System.IO.File.ReadAllBytes(filePath);
                return File(file, "application/force-download", fileName);
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
    }
}
