using coReport.Auth;
using coReport.Models.ProjectModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.ReportModels
{
    public class Report
    {
        public short Id { get; set; }
        public String Title { get; set; }


        public short AuthorId { get; set; }
        public virtual ApplicationUser Author { get; set; }

        public String Text { get; set; }
        public short ProjectId { get; set; }
        public virtual Project Project { get; set; }

        public DateTime Date { get; set; }
        public DateTime EnterTime { get; set; }
        public DateTime ExitTime { get; set; }
        public String AttachmentExtension { get; set; }
        public ICollection<ProjectManager> ProjectManagers { get; set; }
        public ICollection<ManagerReport> ManagerReports { get; set; }
    }
}
