using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.AdminViewModels
{
    public class WarningViewModel
    {
        public String Title { get; set; }
        public String ReceiverName { get; set; }
        public bool IsViewed { get; set; }
        public double ElapsedTime { get; set; }
    }
}
