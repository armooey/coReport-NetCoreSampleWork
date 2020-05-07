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
        public IEnumerable<Report> GetAll();
        public Report Get(short id);
        public int GetReportsCountByDate(DateTime date);
        public IEnumerable<Report> GetByAuthorId(short id);

        public IEnumerable<ProjectManager> GetAllReports(short managerId);
        public Report Add(Report report, IEnumerable<short> managerIds);
        public void Update(Report report, IEnumerable<short> managerIds);
        public void UpdateAttachment(short id, String attachmentExtensions);
        public void Delete(short id);
        public bool IsViewd(short reportId);
        public void SetViewed(short reportId, short managerId);
        public void PreprocessUserDelete(short id);
    }
}
