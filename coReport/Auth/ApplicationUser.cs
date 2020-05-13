using coReport.Models.ManagerModels;
using coReport.Models.MessageModels;
using coReport.Models.ReportModels;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace coReport.Auth
{
    public class ApplicationUser : IdentityUser<short>
    {
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public DateTime RegisterDate { get; set; }
        public bool IsActive { get; set; }
        public String ProfileImageName { get; set; }
        public bool IsBanned { get; set; }
        public DateTime BanEndTime { get; set; }
        public ICollection<Report> Reports { get; set; }
        public ICollection<ManagerReport> ManagerReports { get; set; }
        public ICollection<ProjectManager> ProjectsManaged { get; set; }
        public ICollection<UserManager> Users { get; set; }
        public ICollection<UserManager> Managers { get; set; }
        public ICollection<Message> SentMessages { get; set; }
        public ICollection<UserMessage> ReceivedMessages { get; set; }


    }
}
