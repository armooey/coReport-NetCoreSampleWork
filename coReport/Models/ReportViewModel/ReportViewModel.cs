using coReport.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.ReportViewModel
{
    public class ReportViewModel
    {
        public short Id { get; set; }
        public String Title { get; set; }
        public DateTime Date { get; set; }
        public ApplicationUser Author { get; set; }
        public String ProjectName { get; set; }
        public String Text { get; set; }
        public ApplicationUser ProjectManager { get; set; }
        public DateTime EnterTime { get; set; }
        public DateTime ExitTime { get; set; }
        public bool IsViewed { get; set; }
        public String AttachmentName { get; set; }
    }
}
