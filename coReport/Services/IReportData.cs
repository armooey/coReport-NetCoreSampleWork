using coReport.Auth;
using coReport.Models.ReportModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Services
{
    public interface IReportData
    {
        public Report Get(short id);
        public IEnumerable<Report> GetReportsOfLastSevenDays();
        public IEnumerable<Report> GetByAuthorId(short id);

        public IEnumerable<ProjectManager> GetAllReports(short managerId);
        public Report Add(Report report, IEnumerable<short> managerIds);
        public bool Update(Report report, IEnumerable<short> managerIds);
        public bool UpdateAttachment(short id, String attachmentExtensions);
        public bool Delete(short id);
        public bool SetViewed(short reportId, short managerId);
        public bool PreprocessUserDelete(short id);
        public IEnumerable<Report> GetTodayReportsOfUser(short id);
    }
}
