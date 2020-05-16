using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Services
{
    public interface ILogService
    {
        public void Log(String message, Exception exception);
        public void LogProfileImageHistory(short userId, String imageName);
    }
}
