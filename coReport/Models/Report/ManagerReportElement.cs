using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.Report
{
    public class ManagerReportElement
    {
        public String Text { get; set; }
        public short ReportId { get; set; }
        public virtual Report Report { get; set; }
        public short ManagerReportId { get; set; }
        public virtual ManagerReport ManagerReport { get; set; }
        public bool IsAcceptable { get; set; }
        public bool IsViewable { get; set; }
    }
}
