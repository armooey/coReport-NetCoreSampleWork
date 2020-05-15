using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.ReportModels
{
    public class ReportAttachmentHistory
    {
        public short Id { get; set; }
        public short ReportId { get; set; }
        public String AttachmentName { get; set; }
    }
}
