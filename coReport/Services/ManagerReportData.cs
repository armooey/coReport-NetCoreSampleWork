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
        public ManagerReport Add(ManagerReport report)
        {
            try
            {
                _context.ManagerReports.Add(report);
                _context.SaveChanges();
                CheckUserReportAcceptability(report.ReportId, report.Id);
                return report;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public void Update(ManagerReport report)
        {
            try
            {
                _context.ManagerReports.Update(report);
                _context.SaveChanges();
                CheckUserReportAcceptability(report.ReportId, report.Id);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<ManagerReport> GetAll(short managerId)
        {
            return  _context.ManagerReports.Where(mr => mr.AuthorId == managerId)
                .OrderByDescending(r => r.Date)
                .ToList();
        }



        private void CheckUserReportAcceptability(short reportId, short managerReportId)
        {
            if (!_context.Messages.Any(m => m.HelperId == reportId.ToString() && m.Title == "گزارش غیرقابل قبول"))
            {
                var report = _context.Reports.FirstOrDefault(r => r.Id == reportId);
                var noOfProjectManagers = _context.ProjectManagers.Count(pm => pm.ReportId == reportId);
                var noOfNotAcceptedReports = _context.ManagerReports.Count(mr => mr.ReportId == reportId && mr.IsUserReportAcceptable == false);
                if ((noOfNotAcceptedReports / noOfProjectManagers) > 0.5)
                {
                    var message = new Message
                    {
                        Title = "گزارش غیرقابل قبول",
                        Text = String.Format("گزارش {0} توسط بیشتر از نیمی از مدیران مورد قبول واقع نشد. لطفا هر چه سریعتر نسبت به ویرایش آن اقدام فرمایید.",
                                            report.Title),
                        Type = MessageType.Warning,
                        HelperId = reportId.ToString(),
                        Time = DateTime.Now,
                        SenderId = 1
                    };
                    _context.Messages.Add(message);
                    _context.SaveChanges();
                    _context.UserMessages.Add(new UserMessage { MessageId = message.Id, ReceiverId = report.AuthorId, IsViewd = false });
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
