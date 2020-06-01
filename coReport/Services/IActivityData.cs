using coReport.Models.ActivityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Services
{
    public interface IActivityData
    {
        public void InitializeActivities();
        public bool Add(Activity activity);
        public bool Delete(short id);
        public Activity GetActivity(short id);
        public IEnumerable<Activity> GetParentActivities();
        public IEnumerable<Activity> GetAllActivities();
    }
}
