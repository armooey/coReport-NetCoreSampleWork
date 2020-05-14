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
        public IEnumerable<ManagerReport> GetAll(short managerId);
        public IEnumerable<ManagerReport> GetTodayReports(short managerId);
        public ManagerReport GetManagerReportByUserReportId(short id, short managerId);
        public bool Update(ManagerReport report);
        public IEnumerable<ManagerReport> GetReportsOfLastSevenDays();
    }
}
