using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Date
{
    public class PersianDateTime
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }

        public String GetDate()
        {
            return Year.ToString() + "/" + (Month < 10 ? "0" + Month.ToString() : Month.ToString())
                                    + "/" + (Day < 10 ? "0" + Day.ToString() : Day.ToString());
        }

        public String GetTime()
        {
            return (Hour < 10 ? "0" + Hour.ToString() : Hour.ToString())
                    + ":" + (Minute < 10 ? "0" + Minute.ToString() : Minute.ToString());
        }

        public String GetDayAndMonth()
        {
            return (Month < 10 ? "0" + Month.ToString() : Month.ToString())
                     + "/" + (Day < 10 ? "0" + Day.ToString() : Day.ToString());
        }

        public String GetYearAndMonth()
        {
            return Year.ToString() + "/" + (Month < 10 ? "0" + Month.ToString() : Month.ToString());
        }
    }
}
