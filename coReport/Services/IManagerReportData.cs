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
        public IEnumerable<ManagerReport> GetReportsByTimeSpan(short managerId, DateTime startDate, DateTime endTime);
        public IEnumerable<ManagerReport> GetReportsByDay(short managerId, DateTime date);
        public ManagerReport GetManagerReportByUserReportId(short id, short managerId);
        public bool Update(ManagerReport report);
        public IEnumerable<ManagerReport> GetReportsOfLastSevenDays();
    }
}
