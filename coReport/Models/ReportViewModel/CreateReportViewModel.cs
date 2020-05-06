using coReport.Auth;
using coReport.Models.ManagerModels;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.ReportViewModel
{
    public class CreateReportViewModel
    {

        public short Id { get; set; }
        public List<ApplicationUser> Managers { get; set; }


        [Required(ErrorMessage = "تکمیل فیلد عنوان اجباری است.")]
        [Display(Name = "عنوان")]
        public String Title { get; set; }

        [Required(ErrorMessage = "تکمیل فیلد گزارش اجباری است.")]
        public String Text { get; set; }

        [Required(ErrorMessage = "تکمیل فیلد نام پروژه اجباری است.")]
        [Display(Name ="نام پروژه")]
        public String ProjectName { get; set; }

        [Required(ErrorMessage = "تکمیل فیلد مدیران پروژه اجباری است.")]
        [Display(Name = "مدیران پروژه")]
        public List<short> ProjectManagerIds { get; set; }


        [Required(ErrorMessage = "تکمیل فیلد زمان ورود اجباری است.")]
        [Display(Name = "زمان ورود")]
        [DataType(DataType.Time)]
        public DateTime EnterTime { get; set; }

        [Required(ErrorMessage = "تکمیل فیلد زمان خروج اجباری است.")]
        [Display(Name = "زمان خروج")]
        [DataType(DataType.Time)]
        public DateTime ExitTime { get; set; }

        [Display(Name = "ضمیمه")]
        public IFormFile Attachment { get; set; }

        public String AttachmentName { get; set; }

        public bool IsSubmitedByAdmin { get; set; }
    }
}
