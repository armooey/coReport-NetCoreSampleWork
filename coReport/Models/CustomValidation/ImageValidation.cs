using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.CustomValidation
{
    //Custom Validation for Image Extension
    public class ImageValidation: ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
                return true;

            string[] validExtensions = { "JPG", "JPEG", "PNG" };

            var file = (IFormFile)value;
            var fileExtensions = Path.GetExtension(file.FileName).ToUpper().Replace(".", "");
            return validExtensions.Contains(fileExtensions) && file.ContentType.Contains("image");
        }
    }
}
