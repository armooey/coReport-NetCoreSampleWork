using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.ManageViewModels
{
    public class ChangeEmailViewModel
    {
        public String Username { get; set; }
        [Required(ErrorMessage = "تکمیل فیلد ایمیل اجباری است.")]
        [EmailAddress(ErrorMessage = "لطفا یک ایمیل صحیح وارد کنید.")]
        [Display(Name = "ایمیل")]
        public String Email { get; set; }
    }
}
