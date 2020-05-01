using coReport.Models.MessageViewModels;
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
        public IEnumerable<MessageViewModel> Messages { get; set; }
    }
}
