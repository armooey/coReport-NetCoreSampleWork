using coReport.Auth;
using coReport.Models.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Services
{
    public interface IManagerData
    {
        public void SetManagers(short userId, List<short> managerIds);
        public IEnumerable<ApplicationUser> GetManagers(short userId);
    }
}
