using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.LogModel
{
    public class Log
    {
        public short Id { get; set; }
        public DateTime Date { get; set; }
        public String Exception { get; set; }
        public String Message { get; set; }
    }
}
