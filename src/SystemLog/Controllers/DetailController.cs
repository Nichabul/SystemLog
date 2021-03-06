﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using SystemLog.Models;
using SystemLog.Models.DetailViewModels;
using SystemLog.Services;
using SystemLog.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using OfficeOpenXml;
using System.Globalization;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace SystemLog.Controllers
{
    public class DetailController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext DB;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;
        private IHostingEnvironment _environment;
        public SystemLog.Helper.Utility Helper;
        
        public DetailController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext ContextDb,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ILoggerFactory loggerFactory,
            IHostingEnvironment environment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _logger = loggerFactory.CreateLogger<AccountController>();
            _roleManager = roleManager;
            _environment = environment;
            Helper = new SystemLog.Helper.Utility(signInManager, DB);
            DB = ContextDb;
        }
        [Authorize]
        public IActionResult Index()
        {
            ViewBag.Helper = Helper;
            return View("Index");
        }

        public async Task<IActionResult> Getreport(int month, int year)
        {
            ViewBag.Helper = Helper;
            ViewBag.Month = month;
            ViewBag.Year = year;
            var currentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
           // var Date = await DB.Details.Select(a => a.DetailsCreatedate).FirstOrDefaultAsync();
            ViewBag.Data = DB.Details.Where(a=>a.DetailsUsersId == currentUser.Id).ToList();
            return PartialView("Getreport");
        }
        
        [HttpGet]
        public async Task<IActionResult>  ManageDetails(DateTime date)
        {
            var currentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            ViewBag.UserId = currentUser.Id;
            ViewBag.Helper = Helper;
            ViewBag.date = date;
            ViewBag.GetReport = DB.Details.Where(a => a.DetailsCreatedate == date && a.DetailsUsersId == currentUser.Id).ToList();
            return View("ManageDetails");
        }

        [HttpPost]
        public async Task<IActionResult> ManageDetails(DateTime CreateDate, DateTime[] DetailsCreatedate, string[] DetailsName, String[] DetailWork , DateTime[] DetailsDueDate, int[] DetailsStatus, string[] DetailsNoteProblem, string[] DetailsNoteSolve, string UserId)
        {
            string msg = "";
            try
            {
                if(DB.Details.Where(a=>a.DetailsCreatedate == CreateDate && a.DetailsUsersId == UserId).Count() > 0 )
                {
                    var detailremove = DB.Details.Where(b => b.DetailsCreatedate == CreateDate && b.DetailsUsersId == UserId).ToList();
                    DB.Details.RemoveRange(detailremove);
                    DB.SaveChanges();
                   
                    int FieldCount = DetailWork.Count();
                    for (int i = 0; i < FieldCount; i++)
                    {
                        var model = new Details();
                        model.DetailsCreatedate = DetailsCreatedate[i];
                        model.DetailWork = DetailWork[i];
                        model.DetailsUsersId = UserId;
                        model.DetailsName = DetailsName[i];
                        model.DetailsDueDate = DetailsDueDate[i];
                        model.DetailsStatus = DetailsStatus[i];
                        model.DetailsNoteProblem = DetailsNoteProblem[i];
                        model.DetailsNoteSolve = DetailsNoteSolve[i];

                        DB.Details.Add(model);
                        await DB.SaveChangesAsync();
                        msg = "แก้ข้อมูลสำเร็จ";
                    }
                }
                else
                {
                    int FieldCount = DetailWork.Count();
                    for (int i = 0; i < FieldCount; i++)
                    {
                        var model = new Details();
                        model.DetailsCreatedate = DetailsCreatedate[i];
                        model.DetailWork = DetailWork[i];
                        model.DetailsName = DetailsName[i];
                        model.DetailsUsersId = UserId;
                        model.DetailsDueDate = DetailsDueDate[i];
                        model.DetailsStatus = DetailsStatus[i];
                        model.DetailsNoteProblem = DetailsNoteProblem[i];
                        model.DetailsNoteSolve = DetailsNoteSolve[i];

                        DB.Details.Add(model);
                        await DB.SaveChangesAsync();
                        msg = "บันทึกข้อมูลสำเร็จ";
                    }
                }
            }
            catch (Exception e)
            {
               msg = "Error Is:" + e.InnerException.Message;
            }
            return RedirectToAction("index");
        }

        //[HttpGet]
        //public async Task<IActionResult> DeleteDetail(int Id , string UserId)
        //{
        //    string msg = "";
        //    try
        //    {
        //        var Model = await DB.Details.Where(d=>d.DetailsUsersId == UserId && d.DetailsID == Id).FirstOrDefaultAsync();
        //        DB.Details.RemoveRange(Model);
        //        await DB.SaveChangesAsync();
        //        msg = "Removed";
        //    }
        //    catch (Exception e)
        //    {
        //        msg = "Error is :" + e.Message;
        //    }

        //    return RedirectToAction("ManageDetails");
        //    //return RedirectToAction("ManageDetails", "Detail");
        //}
        

        public async Task<IActionResult> Reportdetail(DateTime date)
        {
            var currentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            ViewBag.Helper = Helper;
            ViewBag.UserId = currentUser.Id;
            ViewBag.date = date;
            //ViewBag ใช้สำหรับข้อมูลที่น้อย
            //ViewBag.GetReport = DB.Details.Where(a => a.DetailsCreatedate == date && a.DetailsUsersId == currentUser.Id).ToList();
            var GetReport = DB.Details.Where(a => a.DetailsCreatedate == date && a.DetailsUsersId == currentUser.Id).ToList();
            return View("Reportdetail", GetReport);
        }

        [Authorize]
        public async Task<FileStreamResult> Export(DateTime Date, string UserId)
        {
            var currentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            string FullName = currentUser.FirstName + " " + currentUser.LastName;

            var CompanyId = DB.Users.Include(b => b.Departments).Where(a => a.Id == currentUser.Id).FirstOrDefault();
            var GetReports = DB.Details.Where(a => a.DetailsUsersId == UserId && a.DetailsCreatedate == Date).ToList();

            var templateFilePath = "";
            if (CompanyId.Departments.DeptCompanyId == 1)
            {
                templateFilePath = Path.Combine(_environment.WebRootPath.ToString(), "ReportTemplate/TemplateLogsystem_TS.xlsx");
            }
            else
            {
                templateFilePath = Path.Combine(_environment.WebRootPath.ToString(), "ReportTemplate/TemplateLogsystem_TT.xlsx");
            }
            //var templateFilePath = Path.Combine(_environment.WebRootPath.ToString(), "ReportTemplate/TemplateLogsystem.xlsx");
            FileInfo templateFile = new FileInfo(templateFilePath);

            var newFilePath = Path.Combine(_environment.WebRootPath.ToString(), "ReportTemplate/TempLogsystem.xlsx");
            FileInfo newFile = new FileInfo(newFilePath);

            using (ExcelPackage package = new ExcelPackage(newFile, templateFile))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

                worksheet.Cells["B2"].Value = FullName;
                worksheet.Cells["B3"].Value = Helper.getDateTHAndTimeShortMonth(Date);
                int Rows = 7;
                if (GetReports.Count() > 0)
                {
                    foreach (var GetReport in GetReports)
                    {
                        worksheet.Cells["A" + Rows].Value = Helper.getDateTHAndTimeShortMonth(GetReport.DetailsCreatedate);
                        worksheet.Cells["B" + Rows].Value = GetReport.DetailsName;
                        worksheet.Cells["C" + Rows].Value = GetReport.DetailWork;
                        worksheet.Cells["D" + Rows].Value = Helper.getDateTHAndTimeShortMonth(GetReport.DetailsDueDate);
                        if (GetReport.DetailsStatus == 100)
                        {
                            worksheet.Cells["E" + Rows].Value = "สำเร็จ";
                        }
                        else
                        {
                            worksheet.Cells["E" + Rows].Value = GetReport.DetailsStatus + "%";
                        }
                        worksheet.Cells["F" + Rows].Value = GetReport.DetailsNoteProblem;
                        worksheet.Cells["G" + Rows].Value = GetReport.DetailsNoteSolve;
                        Rows++;
                    }
                }
                else
                {
                    worksheet.Cells["A7:G7"].Merge = true;
                    worksheet.Cells["A7"].Value = "--- ไม่มีข้อมูลการติดตามสถานะการดำเนินงาน ---";
                }
                package.Save();
            }

            var mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return new FileStreamResult(new FileStream(newFilePath, FileMode.Open), mimeType);
        }


        [HttpGet]
        public async Task<IActionResult> SearchReports(DateTime? StartDate, DateTime? EndDate)
        {

            var currentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));

            if (StartDate == null || StartDate == DateTime.MinValue)
            {
               StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }
            if(EndDate == null || EndDate == DateTime.MinValue)
            {
                var DayInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
                EndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DayInMonth);
            }

            var StartDates = Convert.ToDateTime(StartDate).ToString("dd/MM/yyyy");
            var EndDates = Convert.ToDateTime(EndDate).ToString("dd/MM/yyyy");

            ViewBag.Helper = Helper;
            ViewBag.StartDate = StartDates;
            ViewBag.EndDate = EndDates;
            ViewBag.UserId = currentUser.Id;

            var Searchreport = await DB.Details.Where(a => a.DetailsCreatedate >= StartDate && a.DetailsCreatedate <= EndDate
                                                    && a.DetailsUsersId == currentUser.Id).OrderBy(b=>b.DetailsCreatedate).ToListAsync();

            return View("SearchReports", Searchreport);
        }

        [HttpPost]
        public async Task<IActionResult> SearchReports(string StartDate, string EndDate)
        {
            var currentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));

            ViewBag.UserId = currentUser.Id;
            ViewBag.Helper = Helper;
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;

            DateTime StartingDate = DateTime.ParseExact(StartDate, "dd/MM/yyyy", null);
            DateTime EndingDate = DateTime.ParseExact(EndDate, "dd/MM/yyyy", null);

            var Searchreport = await DB.Details.Where(a => a.DetailsCreatedate >= StartingDate && a.DetailsCreatedate <= EndingDate
                                                      && a.DetailsUsersId == currentUser.Id).OrderBy(b => b.DetailsCreatedate).ToListAsync();
            return View("SearchReports", Searchreport);

        }

     
        [Authorize]
        public async Task<FileStreamResult> Exportreport(string StartDate, string EndDate, string UserId)
        {
            var currentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            UserId = currentUser.Id;
            var CompanyId = DB.Users.Include(b=>b.Departments).Where(a=>a.Id == currentUser.Id).FirstOrDefault();

            DateTime StartingDate = DateTime.ParseExact(StartDate, "dd/MM/yyyy", null);
            DateTime EndingDate = DateTime.ParseExact(EndDate, "dd/MM/yyyy", null);
          
            string FullName = currentUser.FirstName + " " + currentUser.LastName;

            var GetReports = DB.Details.Where(a => a.DetailsUsersId == UserId && a.DetailsCreatedate >= StartingDate && a.DetailsCreatedate <= EndingDate).OrderBy(b => b.DetailsCreatedate).ToList();

            var templateFilePath = "";
            if (CompanyId.Departments.DeptCompanyId == 1)
            {
                templateFilePath = Path.Combine(_environment.WebRootPath.ToString(), "ReportTemplate/TemplateLogsystem_TS.xlsx");
            }
            else
            {
                templateFilePath = Path.Combine(_environment.WebRootPath.ToString(), "ReportTemplate/TemplateLogsystem_TT.xlsx");
            }
    
           
            FileInfo templateFile = new FileInfo(templateFilePath);

            var newFilePath = Path.Combine(_environment.WebRootPath.ToString(), "ReportTemplate/TempLogsystem.xlsx");
            FileInfo newFile = new FileInfo(newFilePath);

            using (ExcelPackage package = new ExcelPackage(newFile, templateFile))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

              
                worksheet.Cells["B2"].Value = FullName;
                worksheet.Cells["B3"].Value = Helper.getDateTHAndTimeShortMonth(StartingDate) + " " +"ถึง" + " " + Helper.getDateTHAndTimeShortMonth(EndingDate);
                int Rows = 7;
                if (GetReports.Count() > 0)
                {
                    foreach (var GetReport in GetReports)
                    {
                        worksheet.Cells["A" + Rows].Value = Helper.getDateTHAndTimeShortMonth(GetReport.DetailsCreatedate);
                        worksheet.Cells["B" + Rows].Value = GetReport.DetailsName;
                        worksheet.Cells["C" + Rows].Value = GetReport.DetailWork;
                        worksheet.Cells["D" + Rows].Value = Helper.getDateTHAndTimeShortMonth(GetReport.DetailsDueDate);
                        if (GetReport.DetailsStatus == 100)
                        {
                            worksheet.Cells["E" + Rows].Value = "สำเร็จ";
                        }
                        else
                        {
                            worksheet.Cells["E" + Rows].Value = GetReport.DetailsStatus + "%";
                        }
                        worksheet.Cells["F" + Rows].Value = GetReport.DetailsNoteProblem;
                        worksheet.Cells["G" + Rows].Value = GetReport.DetailsNoteSolve;
                        Rows++;
                    }
                }
                else
                {
                    worksheet.Cells["A7:G7"].Merge = true;
                    worksheet.Cells["A7"].Value = "--- ไม่มีข้อมูลการติดตามสถานะการดำเนินงาน ---";
                }
                package.Save();
            }

            var mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return new FileStreamResult(new FileStream(newFilePath, FileMode.Open), mimeType);
        }

        //Admin

        //[Authorize("ADMIN")]
        [HttpGet]
        public async Task<IActionResult> SearchReportsAdmin(DateTime? StartDate, DateTime? EndDate, String UserId)
        {
            var currentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));


            if (StartDate == null || StartDate == DateTime.MinValue)
            {
                StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }
            if (EndDate == null || EndDate == DateTime.MinValue)
            {
                var DayInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
                EndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DayInMonth);
            }

            var StartDates = Convert.ToDateTime(StartDate).ToString("dd/MM/yyyy");
            var EndDates = Convert.ToDateTime(EndDate).ToString("dd/MM/yyyy");

            ViewBag.Helper = Helper;
            ViewBag.StartDate = StartDates;
            ViewBag.EndDate = EndDates;
            ViewBag.UserId = UserId;

            ViewBag.Company = new SelectList(DB.Companys.ToList(), "CompanyId", "CompanyName");
            var SearchReportAdmin = await DB.Details.Where(a => a.DetailsCreatedate >= StartDate && a.DetailsCreatedate <= EndDate
                             && a.DetailsUsersId == UserId).OrderBy(b => b.DetailsCreatedate).ToListAsync();
            return View("SearchReportsAdmin", SearchReportAdmin);
        }
        [HttpPost]
        public async Task<IActionResult> SearchReportsAdmin(string StartDate, string EndDate, String UserId)
        {
            ViewBag.UserId = UserId;
            ViewBag.Helper = Helper;
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            ViewBag.Company = new SelectList(DB.Companys.ToList(), "CompanyId", "CompanyName");

            DateTime StartingDate = DateTime.ParseExact(StartDate, "dd/MM/yyyy", null);
            DateTime EndingDate = DateTime.ParseExact(EndDate, "dd/MM/yyyy", null);

            var SearchReportAdmin = await DB.Details.Where(a=> a.DetailsCreatedate >= StartingDate && a.DetailsCreatedate <= EndingDate
                                    && a.DetailsUsersId == UserId).OrderBy(b => b.DetailsCreatedate).ToListAsync();
           

            return View("SearchReportsAdmin", SearchReportAdmin);
        }

        [HttpGet]
        public async  Task<IActionResult> EmployeeList (int CompanyId)
        {
            string Html = "";
            var currentUser = await _userManager.FindByNameAsync(_userManager.GetUserId(User));
            var Model = await DB.Users.Include(a => a.Departments).Where(b => b.Departments.DeptCompanyId == CompanyId).ToListAsync();

            Html += "<select>";
            Html += "<option value=''>---กรุณเลือก---</option>";
            foreach (var User in Model)
            {
                Html += "<option value='" + User.Id + "'>" + User.FirstName+" " + User.LastName + "</option>";
            }
            Html += "</select>";
            return Json(Html);
        }

        [Authorize]
        public async Task<FileStreamResult> ExportreportAdmin(string StartDate, string EndDate, string UserId)
        {
            ViewBag.UserId = UserId;
            DateTime StartingDate = DateTime.ParseExact(StartDate, "dd/MM/yyyy", null);
            DateTime EndingDate = DateTime.ParseExact(EndDate, "dd/MM/yyyy", null);
         
            var User = await DB.Users.Where(a => a.Id == UserId).FirstOrDefaultAsync();
            var CompanyId = DB.Users.Include(b => b.Departments).Where(a => a.Id == User.Id).FirstOrDefault();
            var FullName = User.FirstName + " " + User.LastName;

            var GetReports = await DB.Details.Where(a => a.DetailsCreatedate >= StartingDate && a.DetailsCreatedate <= EndingDate && a.DetailsUsersId == UserId).OrderBy(b => b.DetailsCreatedate).ToListAsync();

            var templateFilePath = "";
            if (CompanyId.Departments.DeptCompanyId == 1)
            {
                templateFilePath = Path.Combine(_environment.WebRootPath.ToString(), "ReportTemplate/TemplateLogsystem_TS.xlsx");
            }
            else
            {
                templateFilePath = Path.Combine(_environment.WebRootPath.ToString(), "ReportTemplate/TemplateLogsystem_TT.xlsx");
            }

           // var templateFilePath = Path.Combine(_environment.WebRootPath.ToString(), "ReportTemplate/TemplateLogsystem.xlsx");
            FileInfo templateFile = new FileInfo(templateFilePath);
            var newFilePath = Path.Combine(_environment.WebRootPath.ToString(), "ReportTemplate/TempLogsystem.xlsx");
            FileInfo newFile = new FileInfo(newFilePath);

            using (ExcelPackage package = new ExcelPackage(newFile, templateFile))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

                worksheet.Cells["B2"].Value = FullName;
                worksheet.Cells["B3"].Value = Helper.getDateTHAndTimeShortMonth(StartingDate) + " " + "ถึง" + " " + Helper.getDateTHAndTimeShortMonth(EndingDate);
                int Rows = 7;
                if (GetReports.Count() > 0)
                {
                    foreach (var GetReport in GetReports)
                    {
                        worksheet.Cells["A" + Rows].Value = Helper.getDateTHAndTimeShortMonth(GetReport.DetailsCreatedate);
                        worksheet.Cells["B" + Rows].Value = GetReport.DetailsName;
                        worksheet.Cells["C" + Rows].Value = GetReport.DetailWork;
                        worksheet.Cells["D" + Rows].Value = Helper.getDateTHAndTimeShortMonth(GetReport.DetailsDueDate);
                        if (GetReport.DetailsStatus == 100)
                        {
                            worksheet.Cells["E" + Rows].Value = "สำเร็จ";
                        }
                        else
                        {
                            worksheet.Cells["E" + Rows].Value = GetReport.DetailsStatus + "%";
                        }
                        worksheet.Cells["F" + Rows].Value = GetReport.DetailsNoteProblem;
                        worksheet.Cells["G" + Rows].Value = GetReport.DetailsNoteSolve;
                        Rows++;
                    }
                }
                else
                {
                    worksheet.Cells["A7:G7"].Merge = true;
                    worksheet.Cells["A7"].Value = "--- ไม่มีข้อมูลการติดตามสถานะการดำเนินงาน ---";
                }
                package.Save();
            }

            var mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return new FileStreamResult(new FileStream(newFilePath, FileMode.Open), mimeType);
        }

    }
}
