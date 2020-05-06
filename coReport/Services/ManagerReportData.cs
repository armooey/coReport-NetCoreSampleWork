using coReport.Data;
using coReport.Models.Message;
using coReport.Models.Report;
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
            }
            catch (Exception e)
            {
                throw e;
            }
            return report;
        }

        public IEnumerable<ManagerReport> GetAll()
        {
            return  _context.ManagerReports
                .Include(r => r.ManagerReportElements)
                .OrderByDescending(r => r.Date)
                .ToList();
        }


        public ManagerReport Get(short id)
        {
            return _context.ManagerReports.Where(mr => mr.Id == id)
              .Include(mr => mr.ManagerReportElements)
                .ThenInclude(mre => mre.Report)
                    .ThenInclude(r => r.Author)
              .ToList().FirstOrDefault();
        }

        public void Delete(short id)
        {
            try
            {
                _context.ManagerReportElements.Where(mre => mre.ManagerReportId == id).Delete();//Delete related manager report elements
                _context.ManagerReports.Where(r => r.Id == id).Delete();
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public void AddManagerReportElements(IEnumerable<ManagerReportElement> elements)
        {
            try
            {
                foreach (var element in elements)
                {
                    //update existing element
                    if (_context.ManagerReportElements
                        .Any(mre => mre.ManagerReportId == element.ManagerReportId && mre.ReportId == element.ReportId))
                    {
                        _context.ManagerReportElements.Where(mre => mre.ManagerReportId == element.ManagerReportId && mre.ReportId == element.ReportId)
                            .Update(mre => new ManagerReportElement { Text = element.Text, IsAcceptable = element.IsAcceptable, IsViewable = element.IsViewable});

                    }
                    else //insert new element
                        _context.ManagerReportElements.Add(element);
                    _context.SaveChanges();
                    CheckUserReportAcceptability(element.ReportId, element.ManagerReportId);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void CheckUserReportAcceptability(short reportId, short managerReportId)
        {
            if (!_context.Messages.Any(m => m.HelperId == reportId.ToString() && m.Title == "گزارش غیرقابل قبول"))
            {
                var report = _context.Reports.FirstOrDefault(r => r.Id == reportId);
                var noOfProjectManagers = _context.ProjectManagers.Count(pm => pm.ReportId == reportId);
                var noOfNotAcceptedReports = _context.ManagerReportElements.Count(mre => mre.ReportId == reportId && mre.IsAcceptable == false);
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

        public ManagerReport GetTodayReport(short authorId)
        {
            return _context.ManagerReports.Where(r => r.Date == DateTime.Now.Date && r.AuthorId == authorId)
                .Include(mr => mr.ManagerReportElements).FirstOrDefault();
        }

        public IEnumerable<ManagerReport> GetAll(short managerId)
        {
            return _context.ManagerReports.Where(mr => mr.AuthorId == managerId)
                .Include(r => r.ManagerReportElements)
                .OrderByDescending(r => r.Date)
                .ToList();
        }

        public int GetReportsCountByDate(DateTime date)
        {
            return _context.ManagerReports.Count(r => r.Date.Date == date);
        }
    }
}
