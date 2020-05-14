using coReport.Auth;
using coReport.Data;
using coReport.Models.ManagerModels;
using coReport.Models.ReportModels;
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
                    _context.ProjectManagers.Add(new ProjectManager { ReportId = report.Id, ManagerId = manager});
                }
                _context.SaveChanges();
                return report;
            }
            catch
            {
                return null;
            }
        }

        public bool Delete(short id)
        {
            try
            {

                if(!_context.ProjectManagers.Any(pm => pm.ReportId == id && pm.IsViewd))
                {
                    _context.ProjectManagers.Where(pm => pm.ReportId == id)
                        .Update(pm => new ProjectManager { IsDeleted = true });//Delete related project manager rows
                    _context.Reports.Where(r => r.Id == id)
                        .Update(r => new Report { IsDeleted = true });
                    _context.SaveChanges();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Report Get(short id)
        {
            return _context.Reports.Where(r => r.Id == id && !r.IsDeleted)
                .Include(r => r.Author)
                .Include(r => r.ManagerReports)
                .Include(r => r.ProjectManagers)
                .Include(r => r.Project)
                .FirstOrDefault();
        }

        public IEnumerable<Report> GetByAuthorId(short id)
        {
            return _context.Reports.Where(r => r.Author.Id == id && !r.IsDeleted)
                .Include(r => r.Author)
                .Include(r => r.Project)
                .Include(r => r.ProjectManagers)
                .OrderByDescending(r=>r.Date).ToList();
        }

        public IEnumerable<ProjectManager> GetAllReports(short managerId)
        {
            return _context.ProjectManagers.Where(pm => pm.ManagerId == managerId && !pm.IsDeleted)
                .Include(pm => pm.Report)
                    .ThenInclude(r => r.Author)
                    .Include(pm => pm.Report.Project)
                    .OrderByDescending(pm => pm.Report.Date);
        }

        


        public bool PreprocessUserDelete(short id)
        {
            try
            {
                //Flag elements that manager will be deleted or elements that report author will be deleted.
                _context.ProjectManagers.Where(pm => pm.ManagerId == id || pm.Report.AuthorId == id)
                    .Update(pm => new ProjectManager { IsDeleted = true});
                //Flag Manager reports that manager report author will be deleted or elements that report author will be deleted.
                _context.ManagerReports.Where(mr => mr.AuthorId == id || mr.Report.AuthorId == id)
                    .Update(mr => new ManagerReport { IsDeleted = true });
                //Flag rows that manager will be deleted or user will be deleted.
                _context.UserManagers.Where(um => um.ManagerId == id || um.UserId == id)
                    .Update(um => new UserManager { IsActive = false});
                //Deleting rows that receiver is this user or sender is the user
                _context.UserMessages.Where(um => um.ReceiverId == id || um.Message.SenderId == id).Delete();
                //Deleting messages that sender is this user
                _context.Messages.Where(m => m.SenderId == id).Delete();
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }


        public bool Update(Report report, IEnumerable<short> managerIds)
        {
            try
            {
                //update managers if no one of them submited report
                if (report.ManagerReports == null || !report.ManagerReports.Any())
                {
                    var projectManagers = _context.ProjectManagers.Where(pm => pm.ReportId == report.Id);
                    foreach (var projectManager in projectManagers)
                        _context.ProjectManagers.Remove(projectManager);
                    foreach (var managerId in managerIds)
                    {
                        _context.ProjectManagers.Add(new ProjectManager { ReportId = report.Id, ManagerId = managerId, IsViewd = false });
                    }
                }
                else //Flag updated reports as unseen
                {
                    foreach (var managerId in managerIds)
                    {
                        _context.ProjectManagers.Where(pm => pm.ReportId == report.Id && pm.ManagerId == managerId)
                            .Update(pm => new ProjectManager { IsViewd = false});
                    }
                }
                //update report
                _context.Reports.Update(report);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }


        public bool UpdateAttachment(short id, string attachmentExtensions)
        {
            try
            {
                _context.Reports.Where(r => r.Id == id).
                    Update(r => new Report { AttachmentExtension = attachmentExtensions });
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }



        public bool SetViewed(short reportId, short managerId)
        {
            try
            {
                var report = _context.ProjectManagers.FirstOrDefault(pm => pm.ReportId == reportId 
                                            && pm.ManagerId == managerId);
                if (report != null && !report.IsViewd)
                {
                    report.IsViewd = true;
                    _context.ProjectManagers.Update(report);
                    _context.SaveChanges();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public IEnumerable<Report> GetReportsOfLastSevenDays()
        {
            return _context.Reports.Where(r => !r.IsDeleted && r.Date.Date >= DateTime.Now.Date.AddDays(-7));
        }

        public IEnumerable<Report> GetTodayReportsOfUser(short id)
        {
            return _context.Reports.Where(r => !r.IsDeleted && r.AuthorId == id && r.Date.Date == DateTime.Now.Date);
        }
    }
}
