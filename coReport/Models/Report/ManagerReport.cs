using coReport.Auth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.Report
{
    public class ManagerReport
    {
        public short Id { get; set; }
        public short AuthorId { get; set; }
        public virtual ApplicationUser Author { get; set; }
        public DateTime Date { get; set; }
        public ICollection<ManagerReportElement> ManagerReportElements { get; set; }
    }
}
