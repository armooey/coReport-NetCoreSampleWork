using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using coReport.CustomValidation;

namespace coReport.Models.ManageViewModels
{
    public class AccountInfoViewModel
    {
        public String Username { get; set; }

        [Display(Name = "عکس پروفایل")]
        [ImageFormatValidation(ErrorMessage = "فرمت مناسبی برای تصویر انتخاب کنید.")]
        [ImageSizeValidation(ErrorMessage = "سایز تصویر انتخابی باید کمتر از 100 کیلوبایت باشد.")]
        public IFormFile Image { get; set; }

        [Display(Name = "شماره تلفن", Prompt =("09xx-xxx-xxxx"))]
        [Phone]
        [ProtectedPersonalData]
        [RegularExpression(@"^\(?([0,9]{2}[0-9]{2})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "فرمت شماره تلفن صحیح نیست.")]

        public String PhoneNumber { get; set; }


        [Required(ErrorMessage = "تکمیل فیلد ایمیل اجباری است.")]
        [EmailAddress(ErrorMessage = "لطفا یک ایمیل صحیح وارد کنید.")]
        [Display(Name = "ایمیل")]
        public String Email { get; set; }

        [Required(ErrorMessage = "تکمیل فیلد نام اجباری است.")]
        [Display(Name = "نام")]
        public String FirstName { get; set; }

        [Required(ErrorMessage = "تکمیل فیلد نام خانوادگی اجباری است.")]
        [Display(Name = "نام خانوادگی")]
        public String LastName { get; set; }

        public bool HasImage { get; set; }
    }
}
