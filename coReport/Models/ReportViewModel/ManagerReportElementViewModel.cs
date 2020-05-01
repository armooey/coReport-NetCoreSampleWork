using coReport.Auth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.ReportViewModel
{
    public class ManagerReportElementViewModel
    {
        public short ReportId { get; set; }
        public String Text { get; set; }

        [Required(ErrorMessage ="مشخص کنید که آیا گزارش قابل قبول بود یا خیر.")]
        public bool IsAccepted { get; set; }

        [Required(ErrorMessage ="مشخص کنید که گزارش توسط کاربر قابل مشاهده باشد یا خیر.")]
        public bool IsViewableByUser { get; set; }
        public String ProjectName { get; set; }
        public ApplicationUser Author { get; set; }

        public TimeSpan WorkHour { get; set; }
    }
}
