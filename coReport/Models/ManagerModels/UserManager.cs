using coReport.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.ManagerModels
{
    public class UserManager
    {
        public short UserId { get; set; }
        public DateTime Date { get; set; }
        public bool IsActive { get; set; }
        public virtual ApplicationUser User { get; set; }
        public short ManagerId { get; set; }
        public virtual ApplicationUser Manager { get; set; }
    }
}
