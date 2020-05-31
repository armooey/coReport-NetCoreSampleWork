using coReport.Auth;
using coReport.Models.ActivityModels;
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
        public short ActivityId { get; set; }
        public virtual Activity Activity { get; set; }
        public short? SubActivityId { get; set; }
        public Activity SubActivity { get; set; }
        public String ActivityApendix { get; set; }
        public String Text { get; set; }
        public short ProjectId { get; set; }
        public virtual Project Project { get; set; }
        public DateTime Date { get; set; }
        public DateTime TaskStartTime { get; set; }
        public DateTime TaskEndTime { get; set; }
        public String AttachmentName { get; set; }
        public short InvalidReportMessageId { get; set; }
        public bool IsDeleted { get; set; }
        public ICollection<ProjectManager> ProjectManagers { get; set; }
        public ICollection<ManagerReport> ManagerReports { get; set; }
    }
}
