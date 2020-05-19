using coReport.Auth;
using coReport.Models.MessageViewModels;
using coReport.Models.ReportModels;
using coReport.Models.ReportViewModel;
using coReport.Operations;
using coReport.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace coReport.Controllers
{
    public class ManagerController: Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private IManagerData _managerData;
        private IReportData _reportData;
        private IManagerReportData _managerReportData;

        public ManagerController(UserManager<ApplicationUser> userManager, IReportData reportData,
            IManagerReportData managerReportData,
            IWebHostEnvironment webHostEnvironment,
            RoleManager<IdentityRole<short>> roleManager,
            IManagerData managerData,
            IMessageService messageService)
        {
            _userManager = userManager;
            _managerData = managerData;
            _reportData = reportData;
            _managerReportData = managerReportData;
        }

        public async Task<IActionResult> ManageReports(String nav)
        {
            var manager = await _userManager.FindByNameAsync(User.Identity.Name);
            var userReports = _reportData.GetAllReports(manager.Id);
            
            var userReportViewModels = new List<ReportViewModel>();

            foreach (var report in userReports)
            {
                userReportViewModels.Add(new ReportViewModel
                {
                    Id = report.Report.Id,
                    Title = report.Report.Title,
                    Author = report.Report.Author,
                    Text = report.Report.Text,
                    ProjectName = report.Report.Project.Title,
                    TaskStartTime = report.Report.TaskStartTime,
                    TaskEndTime = report.Report.TaskEndTime,
                    Date = report.Report.Date.ToHijri(),
                    IsViewed = report.IsViewd
                });
            }
          
            var reportsViewModel = new ReportsManagementViewModel
            {
                ManagerId = manager.Id,
                UserReports = userReportViewModels,
                Messages = new List<MessageViewModel>()
            };
            return View(reportsViewModel);
        }

        //Generates Excel report based on submited manager reports
        public IActionResult GetDailyReport(short managerId)
        {
            var reports = _managerReportData.GetTodayReports(managerId).ToList();
            var stream = new MemoryStream();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(stream))
            {
                //Basic styling of worksheet
                var workSheet = package.Workbook.Worksheets.Add("Sheet1");
                workSheet.View.RightToLeft = true;
                var headerCells = workSheet.Cells["A1:E1"];
                headerCells.Style.Fill.PatternType = ExcelFillStyle.MediumGray;
                headerCells.Style.Fill.BackgroundColor.SetColor(Color.Lime);
                headerCells.Style.Font.Bold = true;
                workSheet.Column(1).Width = 20;
                workSheet.Column(2).Width = 20;
                workSheet.Column(3).Width = 20;
                workSheet.Column(4).Width = 60;
                workSheet.Column(5).Width = 20;
                workSheet.Column(4).Style.WrapText = true;
                workSheet.Cells["A1"].Value = "نام کارمند";
                workSheet.Cells["B1"].Value = "نام پروژه";
                workSheet.Cells["C1"].Value = "ساعت کاری";
                workSheet.Cells["D1"].Value = "گزارش مدیر";
                workSheet.Cells["E1"].Value = "وضعیت گزارش";
                //Filling cells with manager report datas
                for(int i=0;i < reports.Count();i++)
                {
                    var report = reports[i];
                    var name = report.Report.Author.FirstName + " " + report.Report.Author.LastName;
                    var workHour = report.Report.TaskEndTime.Subtract(report.Report.TaskStartTime).ToString("hh\\:mm");
                    workSheet.Cells[i + 2, 1].Value = name;
                    workSheet.Cells[i + 2, 2].Value = report.Report.Project.Title;
                    workSheet.Cells[i + 2, 3].Value = workHour;
                    workSheet.Cells[i + 2, 4].Value = report.Text;
                    workSheet.Cells[i + 2, 5].Value = report.IsUserReportAcceptable == true ? "قابل قبول" : "غیرقابل قبول";
                }
                package.Save();
            }
            stream.Position = 0;

            var fileName = String.Format("{0}.xlsx",DateTime.Now.Date.ToHijri().GetDate());
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        //Generates Monthly report of employee activities
        public IActionResult GetMonthlyReport(short managerId)
        {
            var reports = _managerReportData.GetAll(managerId).ToList();
            var stream = new MemoryStream();
            var employees = _managerData.GetEmployees(managerId).ToList();
            var userReportHashMap = new Dictionary<ApplicationUser, List<Report>>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(stream))
            {
                var workSheet = package.Workbook.Worksheets.Add("Sheet1");
                workSheet.View.RightToLeft = true;
                var today = DateTime.Now.ToHijri();
                var numberOfDaysInMonth = SystemOperations.persianCalender.GetDaysInMonth(today.Year, today.Month);
                //Basic styling of worksheet
                workSheet.Cells[1, 1].Value = "نام کارمند";
                var headerCells = workSheet.Cells[1,2,1,numberOfDaysInMonth+1];
                headerCells.Style.Fill.PatternType = ExcelFillStyle.MediumGray;
                headerCells.Style.Fill.BackgroundColor.SetColor(Color.Lime);
                headerCells.Style.Font.Bold = true;
                workSheet.DefaultColWidth = 10;
                var nameCells = workSheet.Cells[2, 1, employees.Count()+1, 1];
                workSheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(Color.Coral);
                workSheet.Column(1).Width = 20;
                nameCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                nameCells.Style.Fill.BackgroundColor.SetColor(Color.Lime);
                //Setting cell borders
                var allCells = workSheet.Cells[1, 1, employees.Count() + 1, numberOfDaysInMonth + 1];
                allCells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                allCells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                allCells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                allCells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                //Filling cells with days of the month
                for (int i = 1; i <= numberOfDaysInMonth; i++)
                {
                    workSheet.Cells[1, i + 1].Value = (today.Month < 10 ? "0" + today.Month.ToString() : today.Month.ToString()) 
                        + "/" + (i < 10 ? "0" + i.ToString() : i.ToString());
                }
                //Filling cells with default values
                for (int i = 0; i < employees.Count(); i++)
                {
                    userReportHashMap[employees[i]] = new List<Report>(); //Creating Hashmap with empty values
                    //Filling spreadsheet cells with default styling
                    var cells = workSheet.Cells[i + 2, 2, i + 2, today.Day + 1];
                    cells.Value = "00:00";
                    cells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cells.Style.Fill.BackgroundColor.SetColor(Color.Red);
                }
                //Filling hashmap with reports
                for (int i = 0; i < reports.Count(); i++)
                {
                    userReportHashMap[reports[i].Report.Author].Add(reports[i].Report);
                }
                //fill cells with report datas
                int cellIndex = 2;
                foreach(var mapElement in userReportHashMap)
                {

                    workSheet.Cells[cellIndex, 1].Value = mapElement.Key.FirstName + " " + mapElement.Key.LastName;
                    foreach (var report in mapElement.Value)
                    {
                        var reportDate = report.Date.ToHijri();
                        workSheet.Cells[cellIndex, reportDate.Day+1].Value =
                            report.TaskEndTime.Subtract(report.TaskStartTime).ToString("hh\\:mm");
                        workSheet.Cells[cellIndex, reportDate.Day+1].Style.Fill.BackgroundColor.SetColor(Color.DeepSkyBlue);
                    }
                    cellIndex++;
                }
                package.Save();
            }
            stream.Position = 0;

            var fileName = String.Format("{0}.xlsx", DateTime.Now.Date.ToHijri().GetYearAndMonth());
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
