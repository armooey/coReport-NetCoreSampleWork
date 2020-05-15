using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Services
{
    public interface IUserData
    {
        public void AddProfileImage(short userId, String imageName);
    }
}
