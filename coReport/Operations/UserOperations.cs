using coReport.Auth;
using coReport.Models.AccountViewModels;
using coReport.Models.MessageModels;
using coReport.Models.MessageViewModels;
using coReport.Models.ProjectViewModels;
using coReport.Models.ReportViewModel;
using coReport.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace coReport.Operations
{
    public static class UserOperations
    {

        public static async void SaveProfileImage(IWebHostEnvironment webHostEnvironment, IFormFile file, String username )
        {
            if (file != null)
            {
                var imageDirectoryPath = Path.Combine(webHostEnvironment.ContentRootPath, "UserData", "Images");
                // Determine whether the Image directory exists.
                if (!Directory.Exists(imageDirectoryPath))
                {
                    Directory.CreateDirectory(imageDirectoryPath);
                }
                var ImageName = Path.Combine(imageDirectoryPath, String.Format("{0}.jpg", username));
                //Deleting Existing File with the same name
                if (System.IO.File.Exists(ImageName))
                {
                    System.IO.File.Delete(ImageName);
                }
                //Retriving image data from stream and saving to server
                using (var localFile = System.IO.File.Create(ImageName))
                using (var uploadedImage = file.OpenReadStream())
                {
                    await uploadedImage.CopyToAsync(localFile);
                }
            }
        }


        public static async void SaveReportAttachment(IWebHostEnvironment webHostEnvironment, IFormFile file, short userId,
            int savedReportId, String lastSavedExtension = null)
        {
            var fileDirectoryPath = Path.Combine(webHostEnvironment.ContentRootPath, "UserData", "Files");
            // Determine whether the file directory exists.
            if (!Directory.Exists(fileDirectoryPath))
            {
                Directory.CreateDirectory(fileDirectoryPath);
            }
            var filePath = Path.Combine(fileDirectoryPath, String.Format("{0}-{1}{2}", userId, savedReportId, lastSavedExtension));
            //Deleting Existing File with the same name
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            filePath = Path.Combine(fileDirectoryPath, String.Format("{0}-{1}{2}", userId, savedReportId, Path.GetExtension(file.FileName)));
            //Retriving file data from stream and saving to server
            using (var localFile = System.IO.File.Create(filePath))
            using (var uploadedFile = file.OpenReadStream())
            {
                await uploadedFile.CopyToAsync(localFile);
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

        public static List<ProjectViewModel> GetInProgressProjectViewModels(IProjectService projectService)
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
                                String.Concat(userMessage.Message.Sender.FirstName, " ", userMessage.Message.Sender.LastName),
                    Type = userMessage.Message.Type,
                    Time = userMessage.Message.Time,
                    IsViewed = userMessage.IsViewd
                });
            }
            return messageViewModels;
        }

    }
}
