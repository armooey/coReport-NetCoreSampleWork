using coReport.Data;
using coReport.Models.AccountModel;
using coReport.Models.LogModel;
using Microsoft.EntityFrameworkCore;
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

        public IEnumerable<Log> GetAll()
        {
            return _context.Logs.OrderByDescending(l => l.Date);
        }

        public string GetLogMessage(short id)
        {
            return _context.Logs.Where(l => l.Id == id).Select(l => l.Exception).FirstOrDefault();
        }

        public void Log(string message, Exception exception)
        {
            //Clear all changes that caused exception
            _context.ChangeTracker.Entries()
                .Where(e => e.Entity != null).ToList()
                .ForEach(e => e.State = EntityState.Detached);
            //Log Exception
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
