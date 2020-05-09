using coReport.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.ReportModels
{
    public class ManagerReport
    {
        public short Id { get; set; }
        public String Text { get; set; }
        public short ReportId { get; set; }
        public virtual Report Report { get; set; }
        public short AuthorId { get; set; }
        public virtual ApplicationUser Author { get; set; }
        public DateTime Date { get; set; }
        public bool IsUserReportAcceptable { get; set; }
        public bool IsCommentViewableByUser { get; set; }
        public short ReviewMessageId  { get; set; }
    }
}
