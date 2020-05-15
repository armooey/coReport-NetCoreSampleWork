using coReport.Data;
using coReport.Models.AccountModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Services
{
    public class UserData : IUserData
    {
        private ApplicationDbContext _context;

        public UserData( ApplicationDbContext context)
        {
            _context = context;
        }
        public void AddProfileImage(short userId, string imageName)
        {
            try
            {
                _context.ProfileImageHistories.Add(new ProfileImageHistory { UserId = userId, ImageName = imageName});
            }
            catch
            { }
        }
    }
}
