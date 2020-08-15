using coReport.Auth;
using coReport.Models.ActivityModels;
using coReport.Models.HomeViewModels;
using coReport.Models.MessageModels;
using coReport.Models.ReportModels;
using coReport.Models.ReportViewModel;
using coReport.Operations;
using coReport.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
        private IProjectData _projectService;
        private IActivityData _activityData;

        public ReportController(IReportData reportData, IManagerReportData adminReportData,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment,
            IManagerData managerData,
            IMessageService messageService,
            IProjectData projectService,
            IActivityData activityService)
        {
            _reportData = reportData;
            _userManager = userManager;
            _managerReportData = adminReportData;
            _webHostEnvironment = webHostEnvironment;
            _managerData = managerData;
            _messageService = messageService;
            _projectService = projectService;
            _activityData = activityService;
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
                Managers = SystemOperations.GetProjectManagerViewModels(author.Id, _managerData) ,//List of all managers of this user
                Projects = SystemOperations.GetInProgressProjectViewModels(_projectService), //All in progress projects
                Activities = _activityData.GetParentActivities().ToList() //List of main activities
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateReportViewModel model)
        {
            model.Managers = SystemOperations.GetProjectManagerViewModels(model.AuthorId, _managerData);
            model.Projects = SystemOperations.GetInProgressProjectViewModels(_projectService);
            model.Activities = _activityData.GetParentActivities().ToList();
            if (ModelState.IsValid)
            {
                if (model.TaskStartTime >= model.TaskEndTime)
                {
                    ModelState.AddModelError("", "زمان ورود و خروج را بررسی کنید.");
                    return View(model);
                }
                //Save report Attachment
                var report = new Report
                {
                    Title = model.Title,
                    Text = model.Text,
                    ProjectId = model.ProjectId,
                    AuthorId = model.AuthorId,
                    ActivityId = model.ActivityId,
                    SubActivityId = model.SubActivityId,
                    ActivityApendix = model.ActivityApendix,
                    TaskStartTime = model.TaskStartTime,
                    TaskEndTime = model.TaskEndTime,
                    Date = DateTime.Now,
                    AttachmentName = model.AttachmentName
                };
                var savedReport = _reportData.Add(report, model.ProjectManagerIds); //Saving Report
                if (savedReport == null)
                {
                    ModelState.AddModelError("", "مشکل در ایجاد گزارش.");
                    return View(model);
                }
                return RedirectToAction("ManageReports", "Account");
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
            if (report == null)
                return NotFound();
            if(report.Author.UserName != User.Identity.Name)
                return RedirectToAction("AccessDenied", "Home");
            var model = new CreateReportViewModel
            {
                Id = report.Id,
                AuthorId = report.AuthorId,
                ActivityId = report.Activity.Id,
                SubActivityId = report.SubActivityId ?? null,
                ActivityApendix = report.ActivityApendix,
                TaskStartTime = report.TaskStartTime,
                TaskEndTime = report.TaskEndTime,
                ProjectId = report.ProjectId,
                Managers = SystemOperations.GetProjectManagerViewModels(report.AuthorId, _managerData),
                Projects = SystemOperations.GetInProgressProjectViewModels(_projectService),
                Activities = _activityData.GetParentActivities().ToList(), //List of main activities
                ProjectManagerIds = report.ProjectManagers.Select(pm => pm.ManagerId).ToList(),
                Title = report.Title,
                Text = report.Text,
                //If any of project managers submited report with this report
                IsSubmitedByManager = report.ManagerReports != null && report.ManagerReports.Any() ? true : false,
                AttachmentName = report.AttachmentName != null ? report.AttachmentName : null
            };
           

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CreateReportViewModel model)
        {
            model.Managers = SystemOperations.GetProjectManagerViewModels(model.AuthorId, _managerData);
            model.Projects = SystemOperations.GetInProgressProjectViewModels(_projectService);
            model.Activities = _activityData.GetParentActivities().ToList();
            if (ModelState.IsValid)
            {
                if (model.TaskStartTime >= model.TaskEndTime)
                {
                    ModelState.AddModelError("", "زمان شروع و پایان را بررسی کنید.");
                    return View(model);
                }
                //Save report Attachment
                var report = _reportData.Get(model.Id);
                report.Title = model.Title;
                report.Text = model.Text;
                report.ProjectId = model.ProjectId;
                report.ActivityId = model.ActivityId;
                report.SubActivityId = model.SubActivityId == 0 ? null : model.SubActivityId;
                report.ActivityApendix = model.ActivityApendix;
                report.TaskStartTime = model.TaskStartTime;
                report.TaskEndTime = model.TaskEndTime;
                if(model.AttachmentName != null) //preventing from saving null instead of last attachment
                    report.AttachmentName = model.AttachmentName;
                var result = _reportData.Update(report, model.ProjectManagerIds);
                if (!result)
                {
                    ModelState.AddModelError("", "مشکل در بروزرسانی!");
                    return View(model);
                }
                return RedirectToAction("ManageReports", "Account");
            }
            return View(model);
        }

        /*
         * Deleting user report
         */
        public IActionResult Delete(short id)
        {
            var result = _reportData.Delete(id);
            if (result)
                return Json(true);
            return Json(false);
        }

   
        /*
         * View user reports
         */
        [HttpGet]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> CreateManagerReport(short id)
        {
            var report = _reportData.Get(id);
            var manager = await _userManager.FindByNameAsync(User.Identity.Name);
            var managerReport = _managerReportData.GetManagerReportByUserReportId(id, manager.Id);
            if (report == null)
                return NotFound();
            if (!report.ProjectManagers.Any(pm => pm.ManagerId == manager.Id)) //If current user is not manager of loaded report
                return RedirectToAction("AccessDenied", "Home");
            var reportModel = new ReportViewModel
            {
                Id = report.Id,
                Title = report.Title,
                Author = report.Author,
                ProjectName = report.Project.Title,
                ActvivityName = report.Activity.Name,
                SubActivityName = report.SubActivity != null ? report.SubActivity.Name : null,
                Text = report.Text,
                TaskStartTime = report.TaskStartTime,
                TaskEndTime = report.TaskEndTime,
                Date = report.Date.ToHijri(),
                AttachmentName = report.AttachmentName != null ? report.AttachmentName : null
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


        [HttpPost]
        [Authorize(Roles = "Manager")]
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
                        Date = DateTime.Now,
                        Author = manager,
                        Text = model.Text,
                        IsUserReportAcceptable = model.IsAcceptable,
                        IsCommentViewableByUser = model.IsViewableByUser,
                        ReportId = model.UserReport.Id
                    };
                    savedManagerReport = _managerReportData.Add(managerReport);
                    if (savedManagerReport == null)
                    {
                        ModelState.AddModelError("", "مشکل در ثبت گزارش!");
                        return View(model);
                    }
                    //Change the report status to viewed
                    var result = _reportData.SetViewed(model.UserReport.Id, manager.Id);
                    if (!result)
                    {
                        ModelState.AddModelError("", "مشکل در ثبت گزارش!");
                        return View(model);
                    }
                }
                else //means user updating manager report
                {
                    savedManagerReport = managerReport;
                    managerReport.Text = model.Text;
                    managerReport.IsUserReportAcceptable = model.IsAcceptable;
                    managerReport.IsCommentViewableByUser = model.IsViewableByUser;
                    var result = _managerReportData.Update(managerReport);
                    if (result == false)
                    {
                        ModelState.AddModelError("", "مشکل در بروزرسانی!");
                        return View(model);
                    }
                    //Change the report status to viewed
                    var setViewedResult = _reportData.SetViewed(model.UserReport.Id, manager.Id);
                    if (!setViewedResult)
                    {
                        ModelState.AddModelError("", "مشکل در ثبت گزارش!");
                        return View(model);
                    }
                }

                //Notify user if manager report is viewable by user
                if (model.IsViewableByUser)
                {
                    var message = new Message
                    {
                        Title = "گزارش مدیر :: " + model.UserReport.Title,
                        Text = model.Text,
                        Sender = manager,
                        Type = MessageType.Manager_Review_Notification,
                        Time = DateTime.Now,
                    };
                    _messageService.AddManagerReviewMessage(message,savedManagerReport.Id, model.UserReport.Author.Id);
                }
                else
                {
                    _messageService.DeleteManagerReviewMessage(savedManagerReport.Id);
                }
                return RedirectToAction("ManageReports", "Manager");
            }
            return View(model);
        }

        //Called with ajax to fetch subactivities of an activity
        public IActionResult GetSubActivities(short selectedActivityId)
        {
            var activity = _activityData.GetActivity(selectedActivityId);
            var subactivitiesList = new List<KeyValuePair<short, string>>();
            foreach (var subactivity in activity.SubActivities)
                subactivitiesList.Add(new KeyValuePair<short, string>(subactivity.Id, subactivity.Name));
            return Json(subactivitiesList);
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadAttachment(IFormFile attachment)
        {
            var fileName = await SystemOperations.SaveReportAttachment(_webHostEnvironment, attachment);
            if (fileName != null)
            {
                return Ok(fileName);
            }
            return BadRequest();
        }

        [HttpGet("download")]
        public async Task<ActionResult> DownloadReportAttachment(String fileName, String reportTitle)
        {
            var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, "UserData", "Files", fileName);
            try
            {
                byte[] file = await System.IO.File.ReadAllBytesAsync(filePath);
                return File(file, "application/force-download", reportTitle +Path.GetExtension(fileName));
            }
            catch
            {
                return NotFound();
            }
        }

    }
}
