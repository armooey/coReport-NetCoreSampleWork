using coReport.Data;
using coReport.Models.MessageModels;
using coReport.Models.ReportModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Z.EntityFramework.Plus;

namespace coReport.Services
{
    public class ManagerReportData : IManagerReportData
    {
        private ApplicationDbContext _context;
        public ManagerReportData(ApplicationDbContext context)
        {
            _context = context;
        }
        public ManagerReport Add(ManagerReport managerReport)
        {
            try
            {
                _context.ManagerReports.Add(managerReport);
                _context.SaveChanges();
                CheckUserReportAcceptability(managerReport.ReportId);
                return managerReport;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public void Update(ManagerReport managerReport)
        {
            try
            {
                _context.ManagerReports.Update(managerReport);
                _context.SaveChanges();
                CheckUserReportAcceptability(managerReport.ReportId);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<ManagerReport> GetAll(short managerId)
        {
            return  _context.ManagerReports.Where(mr => mr.AuthorId == managerId)
                .Include(r => r.Report)
                    .ThenInclude(r => r.Author)
                    .Include(r => r.Report.Project)
                .OrderByDescending(r => r.Date)
                .ToList();
        }
        public IEnumerable<ManagerReport> GetTodayReports(short managerId)
        {
            return  _context.ManagerReports.Where(mr => mr.AuthorId == managerId && mr.Date.Date == DateTime.Now.Date)
                .Include(r => r.Report)
                    .ThenInclude(r => r.Author)
                    .Include(r => r.Report.Project)
                .OrderByDescending(r => r.Date);
        }



        private void CheckUserReportAcceptability(short reportId)
        {
            const short ADMIN_ID = 1;
            var report = _context.Reports.FirstOrDefault(r => r.Id == reportId);
            var noOfProjectManagers = _context.ProjectManagers.Count(pm => pm.ReportId == reportId);
            var noOfNotAcceptedReports = _context.ManagerReports.Count(mr => mr.ReportId == reportId
                                                                            && mr.IsUserReportAcceptable == false);
            //Check if more than half of managers not accepted report or not
            if ((noOfNotAcceptedReports / noOfProjectManagers) > 0.5)
            {
                //do not create warning if one exists
                if (report.InvalidReportMessageId == 0)
                {
                    var message = new Message
                    {
                        Title = "گزارش غیرقابل قبول",
                        Text = String.Format("گزارش {0} توسط بیشتر از نیمی از مدیران مورد قبول واقع نشد. لطفا هر چه سریعتر نسبت به ویرایش آن اقدام فرمایید.",
                                            report.Title),
                        Type = MessageType.Warning,
                        Time = DateTime.Now,
                        SenderId = ADMIN_ID
                    };
                    _context.Messages.Add(message);
                    _context.SaveChanges();
                    _context.UserMessages.Add(new UserMessage { MessageId = message.Id, ReceiverId = report.AuthorId, IsViewd = false });
                    report.InvalidReportMessageId = message.Id;
                    _context.Reports.Update(report);
                    _context.SaveChanges();

                }
            }
            else
            {
                if(report.InvalidReportMessageId != 0)
                {
                    //Deleting Warning
                    _context.UserMessages.Where(um => um.MessageId == report.InvalidReportMessageId).Delete();
                    _context.Messages.Where(m => m.Id == report.InvalidReportMessageId).Delete();
                    report.InvalidReportMessageId = 0;
                    _context.Reports.Update(report);
                    _context.SaveChanges(); 
                }
            }
        }

        public int GetReportsCountByDate(DateTime date)
        {
            return _context.ManagerReports.Count(mr => mr.Date.Date == date);
        }

        public ManagerReport GetManagerReportByUserReportId(short id, short managerId)
        {
            return _context.ManagerReports.FirstOrDefault(mr => mr.ReportId == id && mr.AuthorId == managerId);
        }


    }
}
