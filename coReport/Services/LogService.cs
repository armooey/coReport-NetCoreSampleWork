using coReport.Data;
using coReport.Models.AccountModel;
using coReport.Models.LogModel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace coReport.Services
{
    public class LogService : ILogService
    {
        private ApplicationDbContext _context;
        private  IHttpContextAccessor _httpContext;
        public LogService(ApplicationDbContext context, IHttpContextAccessor httpContext)
        {
            _context = context;
            _httpContext = httpContext;
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
            var exceptionString = exception.ToString();
            var loggedUser = _httpContext.HttpContext.User?.Identity?.Name;
            if (loggedUser != null)
            {
                exceptionString = exceptionString + "\n******************************************************************";
                exceptionString += "\nLogged In User =====> " + loggedUser;
            }

            var log = new Log
            {
                Date = DateTime.Now,
                Message = message,
                Exception = exceptionString
            };

            _context.Logs.Add(log);
            _context.SaveChanges();
        }

        public void LogProfileImageHistory(short userId, string imageName)
        {
            _context.ProfileImageHistory.Add(new ProfileImageHistory { UserId = userId, ImageName = imageName
                                                                        , Date = DateTime.Now});
        }
    }
}
