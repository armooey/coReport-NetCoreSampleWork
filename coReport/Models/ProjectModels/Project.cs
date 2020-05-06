using coReport.Models.ReportModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.ProjectModels
{
    public class Project
    {
        public short Id { get; set; }
        public String Title { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ICollection<Report> Reports { get; set; }
    }
}
