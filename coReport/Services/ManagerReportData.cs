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
        private ILogService _logger;

        public ManagerReportData(ApplicationDbContext context, ILogService logger)
        {
            _context = context;
            _logger = logger;
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
            catch(Exception e)
            {
                _logger.Log("Error in saving manager report", e);
                return null;
            }
        }
        public bool Update(ManagerReport managerReport)
        {
            try
            {
                _context.ManagerReports.Update(managerReport);
                _context.SaveChanges();
                CheckUserReportAcceptability(managerReport.ReportId);
                return true;
            }
            catch(Exception e)
            {
                _logger.Log("Error in updating manager report", e);
                return false;
            }
        }

        public IEnumerable<ManagerReport> GetReportsByTimeSpan(short managerId, DateTime startDate, DateTime endTime)
        {
            return  _context.ManagerReports.Where(mr => mr.AuthorId == managerId 
                                            && startDate.Date <= mr.Report.Date.Date 
                                            && mr.Report.Date.Date <= endTime.Date && !mr.IsDeleted)
                .Include(r => r.Report)
                    .ThenInclude(r => r.Author)
                    .Include(r => r.Report.Project)
                .OrderByDescending(r => r.Date)
                .ToList();
        }
        public IEnumerable<ManagerReport> GetReportsByDay(short managerId, DateTime date)
        {
            return  _context.ManagerReports.Where(mr => mr.AuthorId == managerId && !mr.IsDeleted
                                                        && mr.Date.Date == date.Date)
                .Include(r => r.Report)
                    .ThenInclude(r => r.Author)
                    .Include(r => r.Report.Project)
                .OrderByDescending(r => r.Date);
        }



        private void CheckUserReportAcceptability(short reportId)
        {
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
                        //Quill text data
                        Text = "{\"ops\":[{\"attributes\":{\"size\":\"15px\",\"font\":\"default\"},\"insert\":\"گزارش \"}," +
                        "{\"attributes\":{\"font\":\"default\",\"size\":\"15px\",\"bold\":true},\"insert\":\" " + report.Title +" \"}," +
                        "{\"attributes\":{\"font\":\"default\",\"size\":\"15px\"},\"insert\":\"توسط بیشتر از نیمی از مدیران مورد قبول واقع نشد.\"}," +
                        "{\"attributes\":{\"align\":\"right\",\"direction\":\"rtl\"},\"insert\":\" \\n \"}," +
                        "{\"attributes\":{\"font\":\"default\",\"size\":\"15px\"},\"insert\":\"لطفا هرچه \"}," +
                        "{\"attributes\":{\"font\":\"default\",\"size\":\"15px\",\"bold\":true},\"insert\":\"سریعتر\"}," +
                        "{\"attributes\":{\"font\":\"default\",\"size\":\"15px\"},\"insert\":\" نسبت به ویرایش این گزارش اقدام فرمایید.\"}," +
                        "{\"attributes\":{\"align\":\"right\",\"direction\":\"rtl\"},\"insert\":\"\\n\"}]}",
                        Type = MessageType.Warning,
                        Time = DateTime.Now
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

        public IEnumerable<ManagerReport> GetReportsOfLastSevenDays()
        {
            return _context.ManagerReports.Where(mr => !mr.IsDeleted && mr.Date.Date >= DateTime.Now.Date.AddDays(-7));
        }

        public ManagerReport GetManagerReportByUserReportId(short id, short managerId)
        {
            return _context.ManagerReports.FirstOrDefault(mr => mr.ReportId == id && mr.AuthorId == managerId);
        }


    }
}
