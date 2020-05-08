using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using coReport.CustomValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace coReport.Models.AccountViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "تکمیل فیلد نام کاربری اجباری است.")]
        [Display(Name = "نام کاربری")]
        public String Username { get; set; }

        [Required(ErrorMessage = "تکمیل فیلد نام اجباری است.")]
        [Display(Name = "نام")]
        public String FirstName { get; set; }

        [Required(ErrorMessage = "تکمیل فیلد نام خانوادگی اجباری است.")]
        [Display(Name = "نام خانوادگی")]
        public String LastName { get; set; }


        [Required(ErrorMessage ="تکمیل فیلد ایمیل اجباری است.")]
        [EmailAddress(ErrorMessage = "لطفا یک ایمیل صحیح وارد کنید.")]
        [Display(Name = "ایمیل")]
        public String Email { get; set; }

        [Required(ErrorMessage = "تکمیل فیلد نقش سیستمی اجباری است.")]
        [Display(Name = "نقش سیستمی")]
        public String Role { get; set; }

        [Display(Name = "عکس پروفایل")]
        [ImageValidation(ErrorMessage = "فرمت مناسبی برای تصویر انتخاب کنید.")]
        public IFormFile Image { get; set; }

        [Required(ErrorMessage = "تکمیل فیلد کلمه عبور اجباری است.")]
        [StringLength(100, ErrorMessage = "کلمه عبور باید حداقل 6 کاراکتر باشد.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "کلمه عبور")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تکرار کلمه عبور")]
        [Compare("Password", ErrorMessage = "کلمه عبور و تکرار کلمه عبور با هم مطابقت ندارند.")]
        public string ConfirmPassword { get; set; }


        //Dummy Property
        public IEnumerable<SelectListItem> Roles { get; set; }
    }
}
