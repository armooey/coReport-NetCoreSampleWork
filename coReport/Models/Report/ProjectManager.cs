﻿using coReport.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.Report
{
    public class ProjectManager
    {
        public short ReportId { get; set; }
        public virtual Report Report { get; set; }
        public short ManagerId { get; set; }
        public virtual ApplicationUser Manager { get; set; }
        public bool IsViewd { get; set; }
    }
}
