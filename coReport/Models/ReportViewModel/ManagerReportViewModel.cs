using coReport.Auth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.ReportViewModel
{
    public class ManagerReportViewModel
    {
        public short Id { get; set; }
        [Required(ErrorMessage ="تکمیل فیلد گزارش اجباری است.")]
        [Display(Name ="گزارش مدیر")]
        public String Text { get; set; }
        [Display(Name = "وضعیت گزارش کارمند")]
        public bool IsAcceptable { get; set; }
        [Display(Name = "مشاهده‌پذیری")]
        public bool IsViewableByUser { get; set; }
        public ReportViewModel UserReport { get; set; }
    }
}
