using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Models.ActivityModels
{
    public class Activity
    {
        public short Id { get; set; }
        public String Name { get; set; }
        public virtual Activity ParentActivity { get; set; }
        public short? ParentActivityId { get; set; }
        public bool IsDeleted { get; set; }
        public IEnumerable<Activity> SubActivities { get; set; }
    }
}
