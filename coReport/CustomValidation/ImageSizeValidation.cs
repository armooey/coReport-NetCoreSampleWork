using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.CustomValidation
{
    public class ImageSizeValidation: ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
                return true;
            var image = (IFormFile)value;
            return image.Length <= 102400; //image size less than 100 kb
        }
    }
}
