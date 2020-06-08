using coReport.Models.AccountViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.ManageViewModels
{
    public class AdministrationViewModel
    {
        public string Username { get; set; }

        [Display(Name = "نقش سیستمی")]
        public String Role { get; set; }

        [Display(Name = "مدیران")]
        [Required(ErrorMessage = "انتخاب مدیران اجباری است.")]
        public List<short> ManagerIds { get; set; }

        [Display(Name ="زمان پایان منع کاربر")]
        [Required]
        public DateTime BanEnd { get; set; }

        [Display(Name ="منع کاربر")]
        public bool IsBanned { get; set; }

        //Dummy Property
        public IEnumerable<SelectListItem> Roles { get; set; }
        public List<UserViewModel> Managers { get; set; }
    }
}
