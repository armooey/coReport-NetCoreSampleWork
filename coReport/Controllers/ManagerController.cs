using coReport.Auth;
using coReport.Models.MessageViewModels;
using coReport.Models.ReportModels;
using coReport.Models.ReportViewModel;
using coReport.Operations;
using coReport.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
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
        public async Task<IActionResult> GetDailyReport(DateTime date, String token)
        {
            //Setting download token to check start of download on client side
            Response.Cookies.Append("downloadToken", token,
                new CookieOptions
                {
                    Expires = DateTime.Now.AddHours(1),
                    IsEssential = true
                });
            var manager = await _userManager.FindByNameAsync(User.Identity.Name);
            var reports = _managerReportData.GetReportsByDay(manager.Id, date).ToList();
            var stream = new MemoryStream();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(stream))
            {
                //Basic styling of worksheet
                var workSheet = package.Workbook.Worksheets.Add("Sheet1");
                workSheet.View.RightToLeft = true;
                var headerCells = workSheet.Cells["A1:G1"];
                headerCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerCells.Style.Fill.BackgroundColor.SetColor(Color.Lime);
                headerCells.Style.Font.Bold = true;
                workSheet.Column(1).Width = 20;
                workSheet.Column(2).Width = 20;
                workSheet.Column(3).Width = 20;
                workSheet.Column(4).Width = 20;
                workSheet.Column(5).Width = 20;
                workSheet.Column(6).Width = 60;
                workSheet.Column(7).Width = 20;
                workSheet.Column(6).Style.WrapText = true;
                workSheet.Cells["A1"].Value = "نام کارمند";
                workSheet.Cells["B1"].Value = "نام پروژه";
                workSheet.Cells["C1"].Value = "فعالیت";
                workSheet.Cells["D1"].Value = "زیرفعالیت";
                workSheet.Cells["E1"].Value = "ساعت کاری";
                workSheet.Cells["F1"].Value = "گزارش مدیر";
                workSheet.Cells["G1"].Value = "وضعیت گزارش";
                //setting cell allinments
                var allCells = workSheet.Cells[1, 1, reports.Count() + 1, 7];
                allCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                allCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Column(6).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                workSheet.Column(6).Style.VerticalAlignment = ExcelVerticalAlignment.Top;


                //Setting cell borders
                allCells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                allCells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                allCells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                allCells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                //Filling cells with manager report datas
                for (int i=0;i < reports.Count();i++)
                {
                    var report = reports[i];
                    var name = report.Report.Author.FirstName + " " + report.Report.Author.LastName;
                    var workHour = report.Report.TaskEndTime.Subtract(report.Report.TaskStartTime).ToString("hh\\:mm");
                    workSheet.Cells[i + 2, 1].Value = name;
                    workSheet.Cells[i + 2, 2].Value = report.Report.Project.Title;
                    workSheet.Cells[i + 2, 3].Value = report.Report.Activity.Name;
                    workSheet.Cells[i + 2, 4].Value = report.Report.SubActivity != null ?
                                                      report.Report.SubActivity.Name : "-";
                    workSheet.Cells[i + 2, 5].Value = workHour;
                    workSheet.Cells[i + 2, 6].Value = SystemOperations.GetTextFromQuillData(report.Text);
                    workSheet.Cells[i + 2, 7].Value = report.IsUserReportAcceptable == true ? "قابل قبول" : "غیرقابل قبول";
                }
                package.Save();
            }
            stream.Position = 0;

            var fileName = String.Format("{0}.xlsx",date.ToHijri().GetDate());
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }




        //Generates cumulative report of employee activities
        public async Task<IActionResult> GetCumulativeReport(DateTime fromDate, DateTime toDate, String token)
        {

            //Setting download token to check start of download on client side
            Response.Cookies.Append("downloadToken", token,
                new CookieOptions
                {
                    Expires = DateTime.Now.AddHours(1),
                    IsEssential = true
                });
            var manager = await _userManager.FindByNameAsync(User.Identity.Name);
            var reports = _managerReportData.GetReportsByTimeSpan(manager.Id, fromDate, toDate).ToList();
            var groupedReports = reports.GroupBy(r => new { r.Report.AuthorId, r.Report.Date.Date });
            var dailyWorkList = new List<UserDailyWork>();
            //Calculating sum of user workhours in day
            foreach (var groupedReport in groupedReports)
            {
                var dailyWork = new UserDailyWork();
                dailyWork.User = groupedReport.First().Report.Author;
                dailyWork.Date = groupedReport.First().Report.Date;
                var workHourSum = new TimeSpan();
                foreach (var report in groupedReport)
                {
                    workHourSum = workHourSum.Add(report.Report.TaskEndTime.Subtract(report.Report.TaskStartTime));
                }
                dailyWork.WorkHour = workHourSum;
                dailyWorkList.Add(dailyWork);
            }
            var stream = new MemoryStream();
            var employees = _managerData.GetEmployees(manager.Id).ToList();
            var userReportHashMap = new Dictionary<ApplicationUser, List<UserDailyWork>>();
            var numberOfDays = (int)toDate.Date.Subtract(fromDate.Date).TotalDays + 1;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(stream))
            {
                var workSheet = package.Workbook.Worksheets.Add("Sheet1");
                workSheet.View.RightToLeft = true;
                //Basic styling of worksheet
                workSheet.Cells[1, 1].Value = "نام کارمند";
                var headerCells = workSheet.Cells[1,2,1,numberOfDays+1];
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
                var allCells = workSheet.Cells[1, 1, employees.Count() + 1, numberOfDays + 1];
                allCells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                allCells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                allCells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                allCells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                allCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                //Filling cells with days of the month
                for (int i = 0; i < numberOfDays; i++)
                {
                    var hijriDate = fromDate.AddDays(i).ToHijri();
                    workSheet.Cells[1, i + 2].Value = (hijriDate.Month < 10 ? "0" + hijriDate.Month.ToString() : hijriDate.Month.ToString()) 
                        + "/" + (hijriDate.Day < 10 ? "0" + hijriDate.Day.ToString() : hijriDate.Day.ToString());
                }
                //Filling cells with default values
                for (int i = 0; i < employees.Count(); i++)
                {
                    userReportHashMap[employees[i]] = new List<UserDailyWork>(); //Creating Hashmap with empty values
                    //Filling spreadsheet cells with default styling
                    var cells = workSheet.Cells[i + 2, 2, i + 2, numberOfDays + 1];
                    cells.Value = "00:00";
                    cells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cells.Style.Fill.BackgroundColor.SetColor(Color.Red);
                }
                //Filling hashmap with dailyWorks
                foreach (var dailyWork in dailyWorkList)
                {
                    userReportHashMap[dailyWork.User].Add(dailyWork);
                }
                //fill cells with report datas
                int cellIndex = 2;
                foreach(var mapElement in userReportHashMap)
                {

                    workSheet.Cells[cellIndex, 1].Value = mapElement.Key.FirstName + " " + mapElement.Key.LastName;
                    foreach (var dailyWork in mapElement.Value)
                    {
                        var dayIndex = (int)dailyWork.Date.Subtract(fromDate.Date).TotalDays;
                        workSheet.Cells[cellIndex, dayIndex+2].Value = dailyWork.WorkHour.ToString("hh\\:mm");
                        workSheet.Cells[cellIndex, dayIndex+2].Style.Fill.BackgroundColor.SetColor(Color.DeepSkyBlue);
                    }
                    cellIndex++;
                }
                package.Save();
            }
            stream.Position = 0;

            var fileName = String.Format("{0}.xlsx", fromDate.ToHijri().GetDate()+" : "+ toDate.ToHijri().GetDate());
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        private class UserDailyWork
        {
            public ApplicationUser User { get; set; }
            public DateTime Date { get; set; }
            public TimeSpan WorkHour { get; set; }
        }
    }
}
