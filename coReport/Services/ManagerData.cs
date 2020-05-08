using coReport.Auth;
using coReport.Data;
using coReport.Models.ManagerModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Services
{
    public class ManagerData : IManagerData
    {
        private ApplicationDbContext _context;

        public ManagerData(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<ApplicationUser> GetEmployees(short managerId)
        {
            return _context.UserManagers.Where(um => um.ManagerId == managerId)
                .Include(um => um.User).Select(um => um.User);
        }

        public IEnumerable<ApplicationUser> GetManagers(short userId)
        {
            return _context.UserManagers.Where(um => um.UserId == userId)
                .Include(um => um.Manager).Select(um => um.Manager);
        }


        public void SetManagers(short userId, List<short> managerIds)
        {
            try
            {
                foreach (var id in managerIds)
                {
                    _context.UserManagers.Add(new UserManager { UserId = userId, ManagerId = id });
                }
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
