using coReport.Models.ReportModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Services
{
    public interface IManagerReportData
    {
        public ManagerReport Add(ManagerReport report);
        public IEnumerable<ManagerReport> GetAll();
        public IEnumerable<ManagerReport> GetAll(short managerId);
        public int GetReportsCountByDate(DateTime date);
        public ManagerReport GetTodayReport(short authorId);
        public ManagerReport Get(short id);
        public void Delete(short id);
        public void AddManagerReportElements(IEnumerable<ManagerReportElement> elements);
    }
}
