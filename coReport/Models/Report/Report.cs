using coReport.Auth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.Report
{
    public class Report
    {
        public short Id { get; set; }
        public String Title { get; set; }


        public short AuthorId { get; set; }
        public virtual ApplicationUser Author { get; set; }

        public String Text { get; set; }
        public String ProjectName { get; set; }

        public DateTime Date { get; set; }
        public DateTime EnterTime { get; set; }
        public DateTime ExitTime { get; set; }
        public String AttachmentExtension { get; set; }
        public ICollection<ProjectManager> ProjectManagers { get; set; }
        public ICollection<ManagerReportElement> ManagerReportElements { get; set; }
    }
}
