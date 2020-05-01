using coReport.Models.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.ReportViewModel
{
    public class ReportsViewModel
    {
        public IEnumerable<ReportViewModel> UserReports { get; set; }

        public IEnumerable<ManagerReportViewModel> ManagerReports { get; set; }
    }
}
