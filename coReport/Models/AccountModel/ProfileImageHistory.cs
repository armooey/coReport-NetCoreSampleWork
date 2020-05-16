using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.AccountModel
{
    public class ProfileImageHistory
    {
        public short Id { get; set; }
        public short UserId { get; set; }
        public DateTime Date { get; set; }
        public String ImageName { get; set; }
    }
}
