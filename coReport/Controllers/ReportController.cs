using coReport.Auth;
using coReport.Models;
using coReport.Models.HomeViewModels;
using coReport.Models.Message;
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
    [Authorize]
    public class ReportController : Controller
    {
        private IReportData _reportData;
        private UserManager<ApplicationUser> _userManager;
        private IManagerReportData _managerReportData;
        private IWebHostEnvironment _webHostEnvironment;
        private IManagerData _managerData;
        private IMessageService _messageService;

        public ReportController(IReportData reportData, IManagerReportData adminReportData,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment,
            IManagerData managerData,
            IMessageService messageService)
        {
            _reportData = reportData;
            _userManager = userManager;
            _managerReportData = adminReportData;
            _webHostEnvironment = webHostEnvironment;
            _managerData = managerData;
            _messageService = messageService;
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
                Managers = _managerData.GetManagers(author.Id).ToList()//List of all managers of this user
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReportViewModel model)
        {
            if (ModelState.IsValid)
            {
                var author = await _userManager.FindByNameAsync(User.Identity.Name);
                if (author == null)
                {
                    ModelState.AddModelError("", "نمی‌توان گزارش را ثبت کرد!");
                    return View(model);
                }
                if (model.EnterTime > model.ExitTime)
                {
                    ModelState.AddModelError("", "زمان ورود و خروج را بررسی کنید.");
                    return View(model);
                }
                var report = new Report
                {
                    Title = model.Title,
                    Text = model.Text,
                    ProjectName = model.ProjectName,
                    Author = author,
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
                        UserOperations.SaveReportAttachment(_webHostEnvironment, model.Attachment, author.UserName, savedReport.Id);
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
        public async Task<IActionResult> Edit(short id)
        {
            var report = _reportData.Get(id);
            var author = await _userManager.FindByNameAsync(User.Identity.Name);
            var model = new CreateReportViewModel
            {
                Id = report.Id,
                EnterTime = report.EnterTime,
                ExitTime = report.ExitTime,
                ProjectName = report.ProjectName,
                Managers = _managerData.GetManagers(author.Id).ToList(),
                ProjectManagerIds = report.ProjectManagers.Select(pm => pm.ManagerId).ToList(),
                Title = report.Title,
                Text = report.Text,
                IsSubmitedByAdmin = report.ManagerReportElements != null && report.ManagerReportElements.Any() ? true : false, //If any of project managers submited report with this report
                AttachmentName = report.AttachmentExtension != null ? 
                                String.Format("{0}-{1}{2}", report.Author.UserName, report.Id, report.AttachmentExtension): null
            };
           

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CreateReportViewModel model)
        {
            if (ModelState.IsValid)
            {
                var author = await _userManager.FindByNameAsync(User.Identity.Name);
                if (author == null)
                {
                    ModelState.AddModelError("", "نمی‌توان گزارش را ثبت کرد!");
                    return View(model);
                }
                if (model.EnterTime > model.ExitTime)
                {
                    ModelState.AddModelError("", "زمان ورود و خروج را بررسی کنید.");
                    return View(model);
                }
                var report = _reportData.Get(model.Id);
                report.Title = model.Title;
                report.Text = model.Text;
                report.ProjectName = model.ProjectName;
                report.EnterTime = model.EnterTime;
                report.ExitTime = model.ExitTime;
                report.Date = DateTime.Now;

                try
                {
                    _reportData.Update(report, model.ProjectManagerIds);
                    //update attachment if user provided new one
                    if (model.Attachment != null)
                    {
                        UserOperations.SaveReportAttachment(_webHostEnvironment, model.Attachment, author.UserName, report.Id, report.AttachmentExtension);
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

   
        [HttpGet]
        [Authorize(Roles = "مدیر")]
        public async Task<IActionResult> CreateManagerReport()
        {
            var manager = await _userManager.FindByNameAsync(User.Identity.Name);
            var managerReport = _managerReportData.GetTodayReport(manager.Id);
            var reports = _reportData.GetTodayReports(manager.Id);
            List<ManagerReportElementViewModel> elements = new List<ManagerReportElementViewModel>();
            foreach (Report report in reports)
            {
                var managerReportElement = report.ManagerReportElements;
                var element = new ManagerReportElement();
                if(managerReportElement != null)
                    element = managerReportElement.FirstOrDefault(mre => mre.ManagerReportId == managerReport.Id);
                elements.Add(new ManagerReportElementViewModel
                {
                    ReportId = report.Id,
                    Author = report.Author,
                    WorkHour = report.ExitTime.Subtract(report.EnterTime),
                    Text = element.Text ?? null,
                    IsAccepted = element != null ? element.IsAcceptable:false,
                    IsViewableByUser = element != null ? element.IsViewable : false,
                    ProjectName = report.ProjectName
                });
            }
            var managerReportModel = new ManagerReportViewModel
            {
                ReportElements = elements

            };
            return View(managerReportModel);

        }

        [HttpPost]
        [Authorize(Roles = "مدیر")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateManagerReport(ManagerReportViewModel model)
        {
            var manager = await _userManager.FindByNameAsync(User.Identity.Name);
            if (ModelState.IsValid)
            {
                var managerReport = _managerReportData.GetTodayReport(manager.Id);
                ManagerReport savedManagerReport;
                if (managerReport == null) //means that user creating a new manager report
                {
                    managerReport = new ManagerReport
                    {
                        Date = DateTime.Now.Date,
                        Author = manager
                    };
                    try
                    {
                        savedManagerReport = _managerReportData.Add(managerReport);
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
                else //means user editing manager report
                {
                    savedManagerReport = managerReport;
                }

                //Saving report Elements
                var managerReportElements = new List<ManagerReportElement>();
                foreach(var element in model.ReportElements)
                {
                    if (element.Text != null)
                    {
                        managerReportElements.Add(new ManagerReportElement { 
                            ManagerReportId = savedManagerReport.Id,
                            ReportId = element.ReportId,
                            Text = element.Text,
                            IsAcceptable = element.IsAccepted,
                            IsViewable = element.IsViewableByUser
                        });
                        //Notify user if manager report is viewable by user
                        if (element.IsViewableByUser)
                        {
                            var message = new Message
                            {
                                Title = "گزارش مدیر",
                                Text = element.Text,
                                Sender = manager,
                                Type = MessageType.Manager_Report_Notification,
                                Time = DateTime.Now,
                                HelperId = String.Concat(savedManagerReport.Id,"-",element.ReportId)
                            };
                            var reportAuthor = _reportData.Get(element.ReportId).Author;
                            _messageService.AddManagerReviewMessage(message, reportAuthor.Id);
                        }
                        else
                        {
                            _messageService.DeleteManagerReviewMessage(String.Concat(savedManagerReport.Id,"-",element.ReportId));
                        }
                    }
                }
                try
                {
                    _managerReportData.AddManagerReportElements(managerReportElements);
                }
                catch (Exception e)
                {
                    var errorModel = new ErrorViewModel
                    {
                        Error = e.Message
                    };
                    return RedirectToAction("Error","Home",errorModel);
                }
                return RedirectToAction("ManageReports", "Manager",new {nav = "unseen" });
            }
            return View(model);
        }



        [Authorize(Roles = "مدیر")]
        public IActionResult ViewManagerReport(short id)
        {
            var managerReport = _managerReportData.Get(id);
            var reportElements = new List<ManagerReportElementViewModel>();
            foreach (var element in managerReport.ManagerReportElements)
            {
                reportElements.Add(new ManagerReportElementViewModel
                {
                    Text = element.Text,
                    Author = element.Report.Author,
                    WorkHour = element.Report.ExitTime.Subtract(element.Report.EnterTime),
                    ProjectName = element.Report.ProjectName
                });
            }
            var model = new ManagerReportViewModel { 
                Date = managerReport.Date,
                ReportElements = reportElements
            };
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
