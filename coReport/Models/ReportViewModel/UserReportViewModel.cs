using coReport.Models.MessageViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.ReportViewModel
{
    public class UserReportViewModel
    {
        public IEnumerable<ReportViewModel> Reports { get; set; }
        public IEnumerable<MessageViewModel> Messages { get; set; }
    }
}
