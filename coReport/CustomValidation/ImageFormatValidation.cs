using coReport.Data;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.CustomValidation
{
    //Custom Validation for Image Extension
    public class ImageFormatValidation: ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
                return true;

            var image = (IFormFile)value;
            var imageExtension = Path.GetExtension(image.FileName).ToUpper().Replace(".", "");
            return AppSettingInMemoryDatabase.IMAGE_FORMATS.Contains(imageExtension) && image.ContentType.Contains("image");
        }
    }
}
