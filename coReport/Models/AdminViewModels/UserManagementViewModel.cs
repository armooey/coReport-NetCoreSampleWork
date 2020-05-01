using coReport.Models.AccountViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.AdminViewModels
{
    public class UserManagementViewModel
    {
        public List<UserViewModel> Users { get; set; }

        [Required(ErrorMessage ="ابتدا مدیران را انتخاب کنید.")]
        public List<short> ManagerIds { get; set; }
    }
}
