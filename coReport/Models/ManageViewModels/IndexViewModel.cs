using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using coReport.CustomValidation;

namespace coReport.Models.ManageViewModels
{
    public class IndexViewModel
    {
        public String Username { get; set; }

        [Display(Name = "عکس پروفایل")]
        [ImageValidation(ErrorMessage = "فرمت مناسبی برای تصویر انتخاب کنید.")]
        public IFormFile Image { get; set; }

        [Display(Name = "شماره تلفن", Prompt =("09xx-xxx-xxxx"))]
        [Phone]
        [ProtectedPersonalData]
        [RegularExpression(@"^\(?([0,9]{2}[0-9]{2})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "فرمت شماره تلفن صحیح نیست.")]

        public String PhoneNumber { get; set; }

        [Required(ErrorMessage = "تکمیل فیلد نام اجباری است.")]
        [Display(Name = "نام")]
        public String FirstName { get; set; }

        [Required(ErrorMessage = "تکمیل فیلد نام خانوادگی اجباری است.")]
        [Display(Name = "نام خانوادگی")]
        public String LastName { get; set; }

        public byte[] ImageByte { get; set; } //Byte data of user image
    }
}
