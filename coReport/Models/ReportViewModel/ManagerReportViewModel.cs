using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.ReportViewModel
{
    public class ManagerReportViewModel
    {
        public short Id { get; set; }
        public DateTime Date { get; set; }
        public List<ManagerReportElementViewModel> ReportElements { get; set; }
    }
}
