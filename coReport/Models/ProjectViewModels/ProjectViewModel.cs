using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.ProjectViewModels
{
    public class ProjectViewModel
    {
        public short Id { get; set; }

        [Required(ErrorMessage ="عنوان پروژه الزامی است.")]
        [Display(Name ="عنوان پروژه")]
        public String Title { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
