using coReport.Auth;
using coReport.Models.MessageViewModels;
using coReport.Models.ReportModels;
using coReport.Models.ReportViewModel;
using coReport.Operations;
using coReport.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace coReport.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController: Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private IManagerData _managerData;
        private IReportData _reportData;
        private IManagerReportData _managerReportData;
        private IMessageService _messageService;

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
            _messageService = messageService;
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
                Messages = SystemOperations.GetMessageViewModels(_messageService, manager.Id)
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
                var headerCells = workSheet.Cells["A1:H1"];
                headerCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerCells.Style.Fill.BackgroundColor.SetColor(Color.Lime);
                headerCells.Style.Font.Bold = true;
                workSheet.Column(1).Width = 20;
                workSheet.Column(2).Width = 20;
                workSheet.Column(3).Width = 20;
                workSheet.Column(4).Width = 20;
                workSheet.Column(5).Width = 20;
                workSheet.Column(6).Width = 60;
                workSheet.Column(7).Width = 60;
                workSheet.Column(8).Width = 20;
                workSheet.Column(6).Style.WrapText = true;
                workSheet.Column(7).Style.WrapText = true;
                workSheet.Cells["A1"].Value = "نام کارمند";
                workSheet.Cells["B1"].Value = "نام پروژه";
                workSheet.Cells["C1"].Value = "فعالیت";
                workSheet.Cells["D1"].Value = "زیرفعالیت";
                workSheet.Cells["E1"].Value = "ساعت کاری";
                workSheet.Cells["F1"].Value = "گزارش کارمند";
                workSheet.Cells["G1"].Value = "گزارش مدیر";
                workSheet.Cells["H1"].Value = "وضعیت گزارش";
                //setting cell allinments
                var allCells = workSheet.Cells[1, 1, reports.Count() + 1, 8];
                allCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                allCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Column(6).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                workSheet.Column(6).Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                workSheet.Column(7).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                workSheet.Column(7).Style.VerticalAlignment = ExcelVerticalAlignment.Top;


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
                    var workHour = report.Report.TaskEndTime.Subtract(report.Report.TaskStartTime);
                    workSheet.Cells[i + 2, 1].Value = name;
                    workSheet.Cells[i + 2, 2].Value = report.Report.Project.Title;
                    workSheet.Cells[i + 2, 3].Value = report.Report.Activity.Name;
                    workSheet.Cells[i + 2, 4].Value = report.Report.SubActivity != null ?
                                                      report.Report.SubActivity.Name : "-";
                    workSheet.Cells[i + 2, 5].Value = workHour;
                    workSheet.Cells[i + 2, 5].Style.Numberformat.Format = "hh:mm";
                    workSheet.Cells[i + 2, 6].Value = SystemOperations.GetTextFromQuillData(report.Report.Text);
                    workSheet.Cells[i + 2, 7].Value = SystemOperations.GetTextFromQuillData(report.Text);
                    workSheet.Cells[i + 2, 8].Value = report.IsUserReportAcceptable == true ? "قابل قبول" : "غیرقابل قبول";
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
            var stream = new MemoryStream();
            var employees = _managerData.GetEmployees(manager.Id).ToList();
            var numberOfDays = (int)toDate.Date.Subtract(fromDate.Date).TotalDays + 1;
            var hijriFromDate = fromDate.ToHijri();
            var hijriToDate = toDate.ToHijri();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(stream))
            {
                foreach (var employee in employees)
                {
                    var employeeName = employee.FirstName + " " + employee.LastName;
                    //Get reports of this employee
                    var employeeReports = reports.Where(um => um.Report.AuthorId == employee.Id)
                        .Select(um => um.Report).OrderBy(r => r.Date).ToList();
                    var workSheet = package.Workbook.Worksheets.Add(employeeName);
                    workSheet.View.RightToLeft = true;
                    workSheet.DefaultColWidth = 25;
                    workSheet.DefaultRowHeight = 30;
                    //Basic styling of worksheet
                    var infoCells = workSheet.Cells[1, 1, 1, 6];
                    infoCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    infoCells.Style.Fill.BackgroundColor.SetColor(Color.DeepSkyBlue);
                    infoCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    infoCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    infoCells.Merge = true;
                    infoCells.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
                    infoCells.Value = "گزارش فعالیت‌های " + employeeName + " از تاریخ " +
                                        hijriFromDate.GetDate() + " تا " + hijriToDate.GetDate();

                    if (employeeReports.Any())
                    {
                        var headerCells = workSheet.Cells[2, 1, 2, 6];
                        headerCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        headerCells.Style.Fill.BackgroundColor.SetColor(Color.LimeGreen);
                        headerCells.Style.Font.Bold = true;

                        workSheet.Cells["A2"].Value = "تاریخ گزارش";
                        workSheet.Cells["B2"].Value = "نام گزارش";
                        workSheet.Cells["C2"].Value = "نام پروژه";
                        workSheet.Cells["D2"].Value = "فعالیت";
                        workSheet.Cells["E2"].Value = "زیرفعالیت";
                        workSheet.Cells["F2"].Value = "ساعت کاری";
                        //Setting cell borders and alignmnet
                        var allCells = workSheet.Cells[2, 1, employeeReports.Count() + 2, 6];
                        allCells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        allCells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        allCells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        allCells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        allCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        allCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                        //fill cells with report datas
                        for (int i = 0; i < employeeReports.Count(); i++)
                        {
                            workSheet.Cells[i + 3, 1].Value = employeeReports[i].Date.ToHijri().GetDate();
                            workSheet.Cells[i + 3, 1].Style.Numberformat.Format = "yyyy/MM/dd";
                            workSheet.Cells[i + 3, 2].Value = employeeReports[i].Title;
                            workSheet.Cells[i + 3, 3].Value = employeeReports[i].Project.Title;
                            workSheet.Cells[i + 3, 4].Value = employeeReports[i].Activity.Name;
                            workSheet.Cells[i + 3, 5].Value = employeeReports[i].SubActivity != null ?
                                                              employeeReports[i].SubActivity.Name : "-";
                            workSheet.Cells[i + 3, 6].Value = employeeReports[i].TaskEndTime.Subtract(employeeReports[i].TaskStartTime);
                            workSheet.Cells[i + 3, 6].Style.Numberformat.Format = "hh:mm";
                        }
                        workSheet.Cells[employeeReports.Count() + 3, 5].Value = "مجموع ساعات کاری";
                        var sumCell = workSheet.Cells[employeeReports.Count() + 3, 6];
                        sumCell.Formula = "Sum(" + workSheet.Cells[3, 6].Address +
                            ":" + workSheet.Cells[employeeReports.Count() + 2, 6].Address + ")";
                        sumCell.Style.Numberformat.Format = "hh:mm";
                        var sumCells = workSheet.Cells[employeeReports.Count() + 3, 5, employeeReports.Count() + 3, 6];
                        sumCells.Style.Font.Bold = true;
                        sumCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        sumCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        sumCells.Style.Border.Left.Style = ExcelBorderStyle.Medium;
                        sumCells.Style.Border.Right.Style = ExcelBorderStyle.Medium;
                        sumCells.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
                    }
                    else
                    {
                        var noReportCells = workSheet.Cells[2, 1, 2, 6];
                        noReportCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        noReportCells.Style.Fill.BackgroundColor.SetColor(Color.OrangeRed);
                        noReportCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        noReportCells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        noReportCells.Merge = true;
                        noReportCells.Value = "گزارشی موجود نیست.";
                    }
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
