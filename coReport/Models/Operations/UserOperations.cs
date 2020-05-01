using coReport.Auth;
using coReport.Models.ReportViewModel;
using coReport.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace coReport.Models.Operations
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


        public static async void SaveReportAttachment(IWebHostEnvironment webHostEnvironment, IFormFile file, String username,
            int savedReportId, String lastSavedExtension = null)
        {
            var fileDirectoryPath = Path.Combine(webHostEnvironment.ContentRootPath, "UserData", "Files");
            // Determine whether the file directory exists.
            if (!Directory.Exists(fileDirectoryPath))
            {
                Directory.CreateDirectory(fileDirectoryPath);
            }
            var filePath = Path.Combine(fileDirectoryPath, String.Format("{0}-{1}{2}", username, savedReportId, lastSavedExtension));
            //Deleting Existing File with the same name
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            filePath = Path.Combine(fileDirectoryPath, String.Format("{0}-{1}{2}", username, savedReportId, Path.GetExtension(file.FileName)));
            //Retriving file data from stream and saving to server
            using (var localFile = System.IO.File.Create(filePath))
            using (var uploadedFile = file.OpenReadStream())
            {
                await uploadedFile.CopyToAsync(localFile);
            }
        }
    }
}
