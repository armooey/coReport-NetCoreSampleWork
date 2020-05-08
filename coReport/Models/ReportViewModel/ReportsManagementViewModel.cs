using coReport.Models.MessageViewModels;
using coReport.Models.ReportModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.ReportViewModel
{
    public class ReportsManagementViewModel
    {
        public short ManagerId { get; set; }
        public IEnumerable<ReportViewModel> UserReports { get; set; }

        public IEnumerable<MessageViewModel> Messages { get; set; }
    }
}
