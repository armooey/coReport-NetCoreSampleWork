using coReport.Auth;
using coReport.Models.AccountViewModels;
using coReport.Models.ManagerModels;
using coReport.Models.ProjectViewModels;
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
        public List<UserViewModel> Managers { get; set; }
        public List<ProjectViewModel> Projects { get; set; }

        public short AuthorId { get; set; }
        [Required(ErrorMessage = "تکمیل فیلد عنوان اجباری است.")]
        [Display(Name = "عنوان")]
        public String Title { get; set; }

        [Required(ErrorMessage = "تکمیل فیلد گزارش اجباری است.")]
        public String Text { get; set; }

        [Required(ErrorMessage = "تکمیل فیلد پروژه اجباری است.")]
        [Display(Name ="پروژه")]
        public short ProjectId { get; set; }

        [Required(ErrorMessage = "تکمیل فیلد مدیران پروژه اجباری است.")]
        [Display(Name = "مدیران پروژه")]
        public List<short> ProjectManagerIds { get; set; }



        [Required(ErrorMessage = "تکمیل فیلد زمان شروع کار اجباری است.")]
        [Display(Name = "زمان شروع کار")]
        [DataType(DataType.Time)]
        public DateTime TaskStartTime { get; set; }

        [Required(ErrorMessage = "تکمیل فیلد زمان پایان کار اجباری است.")]
        [Display(Name = "زمان پایان کار")]
        [DataType(DataType.Time)]
        public DateTime TaskEndTime { get; set; }

        [Display(Name = "ضمیمه")]
        public IFormFile Attachment { get; set; }

        public String AttachmentName { get; set; }

        public bool IsSubmitedByManager { get; set; }
    }
}
