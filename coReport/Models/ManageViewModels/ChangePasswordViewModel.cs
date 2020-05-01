using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.ManageViewModels
{
    public class ChangePasswordViewModel
    {
        public String Username { get; set; }

        [Required(ErrorMessage = "تکمیل فیلد کلمه عبور فعلی اجباری است.")]
        [DataType(DataType.Password)]
        [Display(Name = "کلمه عبور فعلی")]
        public String CurrentPassword { get; set; }

        [Required(ErrorMessage = "تکمیل فیلد کلمه عبور جدید اجباری است.")]
        [StringLength(100, ErrorMessage = "کلمه عبور باید حداقل 6 کاراکتر باشد.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "کلمه عبور جدید")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تکرار کلمه عبور جدید")]
        [Compare("Password", ErrorMessage = "کلمه عبور و تکرار کلمه عبور با هم مطابقت ندارند.")]
        public string ConfirmPassword { get; set; }
    }
}
