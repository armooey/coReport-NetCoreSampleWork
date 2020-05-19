﻿using coReport.Auth;
using coReport.Data;
using coReport.Date;
using coReport.Models.AccountViewModels;
using coReport.Models.MessageModels;
using coReport.Models.MessageViewModels;
using coReport.Models.ProjectViewModels;
using coReport.Models.ReportViewModel;
using coReport.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Operations
{
    public static class SystemOperations
    {
        public static readonly PersianCalendar persianCalender = new PersianCalendar();

        public static async Task<string> SaveProfileImage(IWebHostEnvironment webHostEnvironment, IFormFile file)
        {
            var imageDirectoryPath = Path.Combine(webHostEnvironment.ContentRootPath, "UserData", "Images");
            // Determine whether the Image directory exists.
            if (!Directory.Exists(imageDirectoryPath))
            {
                Directory.CreateDirectory(imageDirectoryPath);
            }
            var imageId = Guid.NewGuid().ToString();
            var ImageName = Path.Combine(imageDirectoryPath, String.Format("{0}.jpg", imageId));
            try
            {
                //Retriving image data from stream and saving to server
                using (var localFile = System.IO.File.Create(ImageName))
                using (var uploadedImage = file.OpenReadStream())
                {
                    await uploadedImage.CopyToAsync(localFile);
                }
            }
            catch
            {
                return null;
            }
            return imageId;
        }


        public static async Task<string> SaveReportAttachment(IWebHostEnvironment webHostEnvironment, IFormFile file)
        {
            var fileName = Guid.NewGuid().ToString();
            var fileDirectoryPath = Path.Combine(webHostEnvironment.ContentRootPath, "UserData", "Files");
            // Determine whether the file directory exists.
            if (!Directory.Exists(fileDirectoryPath))
            {
                Directory.CreateDirectory(fileDirectoryPath);
            }
            //Deleting Existing File with the same name
            try
            {
                fileName = fileName + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(fileDirectoryPath, fileName);
                //Retriving file data from stream and saving to server
                using (var localFile = System.IO.File.Create(filePath))
                using (var uploadedFile = file.OpenReadStream())
                {
                    await uploadedFile.CopyToAsync(localFile);
                }
                return fileName;
            }
            catch
            {
                return null;
            }
        }
        public static void DeleteReportAttachment(IWebHostEnvironment webHostEnvironment, String fileName)
        {
            var filePath = Path.Combine(webHostEnvironment.ContentRootPath, "UserData", "Files", fileName);
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch
            {
                
            }
        }

        public static List<UserViewModel> GetProjectManagerViewModels(short userId, IManagerData managerData)
        {
            var managers = managerData.GetManagers(userId);
            var managerViewModels = new List<UserViewModel>();
            foreach (var manager in managers)
            {
                managerViewModels.Add(new UserViewModel
                {
                    FirstName = manager.FirstName,
                    LastName = manager.LastName,
                    Id = manager.Id
                });
            }
            return managerViewModels;
        }

        public static List<ProjectViewModel> GetInProgressProjectViewModels(IProjectData projectService)
        {
            var projects = projectService.GetInProgressProjects();
            var projectViewModels = new List<ProjectViewModel>();
            foreach (var project in projects)
            {
                projectViewModels.Add(new ProjectViewModel
                {
                    Id = project.Id,
                    Title = project.Title
                });
            }
            return projectViewModels;
        }
        public static List<MessageViewModel> GetMessageViewModels(IMessageService messageService, short userId)
        {
            var messages = messageService.GetReceivedMessages(userId);
            var messageViewModels = new List<MessageViewModel>();
            foreach (var userMessage in messages)
            {
                messageViewModels.Add(new MessageViewModel
                {
                    Id = userMessage.Message.Id,
                    Title = userMessage.Message.Title,
                    Text = userMessage.Message.Text,
                    AuthorName = userMessage.Message.Type == MessageType.System_Notification ? "پیام سیستمی" :
                                userMessage.Message.Sender.FirstName + " " + userMessage.Message.Sender.LastName,
                    Type = userMessage.Message.Type,
                    Time = userMessage.Message.Time.ToHijri(),
                    IsViewed = userMessage.IsViewd
                });
            }
            return messageViewModels;
        }

        public static List<SelectListItem> GetRolesSelectList(this RoleManager<IdentityRole<short>> roleManager)
        {
            return new List<SelectListItem>
                {
                    new SelectListItem{ Text = AppSettingInMemoryDatabase.EMPLOYEE_ROLE_NAME, Value = "Employee"},
                    new SelectListItem{ Text = AppSettingInMemoryDatabase.MANAGER_ROLE_NAME, Value = "Manager" }
                };
        }

        public static PersianDateTime ToHijri(this DateTime time)
        {
            return new PersianDateTime{Year = persianCalender.GetYear(time),
                                       Month = persianCalender.GetMonth(time),
                                       Day = persianCalender.GetDayOfMonth(time),
                                       Hour = time.Hour, Minute = time.Minute, Second = time.Second };
        }
    }
}
