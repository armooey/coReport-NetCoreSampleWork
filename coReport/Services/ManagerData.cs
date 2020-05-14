using coReport.Auth;
using coReport.Data;
using coReport.Models.ManagerModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

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
            return _context.UserManagers.Where(um => um.ManagerId == managerId && um.IsActive == true)
                .Include(um => um.User).Select(um => um.User);
        }

        public IEnumerable<ApplicationUser> GetManagers(short userId)
        {
            return _context.UserManagers.Where(um => um.UserId == userId && um.IsActive == true)
                .Include(um => um.Manager).Select(um => um.Manager);
        }


        public bool SetManagers(short userId, List<short> managerIds)
        {
            try
            {
                foreach (var id in managerIds)
                {

                    _context.UserManagers.Add(new UserManager
                    {
                        UserId = userId,
                        ManagerId = id,
                        Date = DateTime.Now,
                        IsActive = true
                    });
                }
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public void UpdateManagers(short userId, List<short> managerIds)
        {
            try
            {
                //Deactive all rows related to this user
                _context.UserManagers.Where(um => um.UserId == userId).Update(um => new UserManager { IsActive = false });
                foreach (var id in managerIds)
                {
                    if (_context.UserManagers.Any(um => um.UserId == userId && um.ManagerId == id))
                    {
                        //If row exists activate the row
                        _context.UserManagers.Where(um => um.UserId == userId && um.ManagerId == id)
                            .Update(um => new UserManager { IsActive = true });
                    }
                    else
                    {
                        _context.UserManagers.Add(new UserManager
                        {
                            UserId = userId,
                            ManagerId = id,
                            Date = DateTime.Now,
                            IsActive = true
                        });
                    }
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
