using coReport.Models.ReportModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.ReportViewModel
{
    public class UserAndManagerReportViewModel
    {
        public IEnumerable<ReportViewModel> UserReports { get; set; }

        public IEnumerable<ManagerReportViewModel> ManagerReports { get; set; }
    }
}
