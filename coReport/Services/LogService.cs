using coReport.Data;
using coReport.Models.AccountModel;
using coReport.Models.LogModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Services
{
    public class LogService : ILogService
    {
        private ApplicationDbContext _context;

        public LogService(ApplicationDbContext context)
        {
            _context = context;
        }
        public void Log(string message, Exception exception)
        {
            _context.Logs.Add(new Log { 
                Date = DateTime.Now,
                Message = message,
                Exception = exception.ToString()
            });
            _context.SaveChanges();
        }

        public void LogProfileImageHistory(short userId, string imageName)
        {
            _context.ProfileImageHistory.Add(new ProfileImageHistory { UserId = userId, ImageName = imageName
                                                                        , Date = DateTime.Now});
        }
    }
}
