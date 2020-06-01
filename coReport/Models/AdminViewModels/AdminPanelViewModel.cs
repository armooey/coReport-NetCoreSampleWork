using coReport.Models.ActivityModels;
using coReport.Models.MessageViewModels;
using coReport.Models.ProjectViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.AdminViewModels
{
    public class AdminPanelViewModel
    {
        public List<String> Days { get; set; }
        public List<int> UsersReportCount { get; set; }
        public List<int> ManagersReportCount { get; set; }
        public List<ProjectViewModel> Projects { get; set; }
        public List<MessageViewModel> Messages { get; set; }
        public List<WarningViewModel> Warnings { get; set; }
        public List<Activity> Activities { get; set; }
    }
}
