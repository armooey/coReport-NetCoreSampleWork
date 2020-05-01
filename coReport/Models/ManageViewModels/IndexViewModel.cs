using coReport.Auth;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.ManageViewModels
{
    public class IndexViewModel
    {
        public String Username { get; set; }

        [Display(Name = "شماره تلفن")]
        [Phone]
        [ProtectedPersonalData]
        public String PhoneNumber { get; set; }

        [Required(ErrorMessage = "تکمیل فیلد نام اجباری است.")]
        [Display(Name = "نام")]
        public String FirstName { get; set; }

        [Required(ErrorMessage = "تکمیل فیلد نام خانوادگی اجباری است.")]
        [Display(Name = "نام خانوادگی")]
        public String LastName { get; set; }
    }
}
