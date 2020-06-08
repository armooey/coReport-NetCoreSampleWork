using coReport.Auth;
using coReport.Models.ManagerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Services
{
    public interface IManagerData
    {
        public bool SetManagers(short userId, List<short> managerIds);
        public IEnumerable<ApplicationUser> GetEmployees(short managerId);
        public IEnumerable<ApplicationUser> GetManagers(short userId);
        public bool DeleteManagers(short userId);
        public bool UpdateManagers(short userId, IEnumerable<short> managerIds);
    }
}
