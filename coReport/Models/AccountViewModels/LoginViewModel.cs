using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.AccountViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "تکمیل فیلد نام کاربری اجباری است.")]
        [Display(Name = "نام کاربری")]
        public string Username { get; set; }

        [Required(ErrorMessage = "تکمیل فیلد کلمه عبور اجباری است.")]
        [DataType(DataType.Password)]
        [Display(Name = "کلمه عبور")]
        public string Password { get; set; }

        [Display(Name = "مرا به خاطر بسپار")]
        public bool RememberMe { get; set; }
    }
}
