using coReport.Auth;
using coReport.Data;
using coReport.Models.Report;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Z.EntityFramework.Plus;

namespace coReport.Services
{
    public class ReportData : IReportData
    {
        private ApplicationDbContext _context;
        public ReportData(ApplicationDbContext context)
        {
            _context = context;
        }

        public Report Add(Report report, IEnumerable<short> managerIds)
        {
            try
            {
                _context.Reports.Add(report);
                _context.SaveChanges();
                foreach (var manager in managerIds)
                {
                    _context.ProjectManagers.Add(new ProjectManager { ReportId = report.Id, ManagerId = manager, IsViewd = false });
                }
                _context.SaveChanges();
                return report;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void Delete(short id)
        {
            try
            {
                _context.ProjectManagers.Where(pm => pm.ReportId == id).Delete();//Delete related project manager rows
                _context.ManagerReportElements.Where(mre => mre.ReportId == id).Delete();//Delete related manager report element
                _context.Reports.Where(r => r.Id == id).Delete();
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public Report Get(short id)
        {
            return _context.Reports.Where(r => r.Id == id)
                .Include(r => r.Author)
                .Include(r => r.ManagerReportElements)
                .Include(r => r.ProjectManagers)
                .FirstOrDefault();
        }

        public IEnumerable<Report> GetAll()
        {
            return _context.Reports
                .Include(r => r.Author)
                .OrderBy(r => r.Date).ToList();
        }

        public IEnumerable<Report> GetByAuthorId(short id)
        {
            return _context.Reports.Where(r => r.Author.Id == id).
                Include(r => r.Author).
                OrderBy(r=>r.Date).ToList();
        }

        public IEnumerable<Report> GetTodayReports(short managerId)
        {
            return _context.ProjectManagers.Where(pm => pm.ManagerId == managerId && pm.Report.Date.Date == DateTime.Now.Date)
                .Include(pm => pm.Report)
                 .ThenInclude(r => r.Author)
                .Select(pm => pm.Report);
        }


        public IEnumerable<Report> GetUnseenReports(short managerId)
        {
            return _context.ProjectManagers.Where(pm => pm.ManagerId == managerId && pm.IsViewd == false)
                .Include(pm => pm.Report)
                 .ThenInclude(r => r.Author)
                .Select(pm => pm.Report);
        }

        


        public void PreprocessUserDelete(short id)
        {
            try
            {
                //Deleting elements that manager will be deleted or elements that report author will be deleted.
                _context.ProjectManagers.Where(pm => pm.ManagerId == id || pm.Report.AuthorId == id).Delete();
                //Deleting Manager report elements that manager report author will be deleted or elements that report author will be deleted.
                _context.ManagerReportElements.Where(mre => mre.ManagerReport.AuthorId == id || mre.Report.AuthorId == id).Delete();
                //Deleting rows that manager will be deleted or author will be deleted.
                _context.UserManagers.Where(um => um.ManagerId == id || um.UserId == id).Delete();
                //Deleting rows that receiver is this user or sender is the user
                _context.UserMessages.Where(um => um.ReceiverId == id || um.Message.SenderId == id).Delete();
                //Deleting messages that sender is this user
                _context.Messages.Where(m => m.SenderId == id).Delete();
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public void Update(Report report, IEnumerable<short> managerIds)
        {
            try
            {
                //update managers if no one of them submited report
                if (report.ManagerReportElements == null || !report.ManagerReportElements.Any())
                {
                    var projectManagers = _context.ProjectManagers.Where(pm => pm.ReportId == report.Id);
                    foreach(var projectManager in projectManagers)
                        _context.ProjectManagers.Remove(projectManager);
                    foreach (var manager in managerIds)
                    {
                        _context.ProjectManagers.Add(new ProjectManager { ReportId = report.Id, ManagerId = manager, IsViewd = false });
                    }
                }
                //update report
                _context.Reports.Update(report);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public void UpdateAttachment(short id, string attachmentExtensions)
        {
            try
            {
                _context.Reports.Where(r => r.Id == id).
                    Update(r => new Report { AttachmentExtension = attachmentExtensions });
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
        }



        public void SetViewed(short reportId, short managerId)
        {
            var report = _context.ProjectManagers.Where(pm => pm.ReportId == reportId && pm.ManagerId == managerId);
            if (!report.FirstOrDefault().IsViewd)
            {
                report.Update(r => new ProjectManager { IsViewd = true });
                try
                {
                    _context.SaveChanges();
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public int GetReportsCountByDate(DateTime date)
        {
            return _context.Reports.Count(r => r.Date.Date == date);
        }
    }
}
