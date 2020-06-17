using coReport.Date;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.LogViewModel
{
    public class LogViewModel
    {
        public short Id { get; set; }
        public String Message { get; set; }
        public PersianDateTime Date { get; set; }
    }
}
