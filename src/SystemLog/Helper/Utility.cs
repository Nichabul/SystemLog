using System;
using SystemLog.Data;
using Microsoft.AspNetCore.Hosting;
using SystemLog.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using SystemLog.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace SystemLog.Helper
{
    public class Utility 
    {
        private readonly ApplicationDbContext DB;
        private readonly UserManager<ApplicationUser> UserManager;
        private readonly SignInManager<ApplicationUser> SignInManager;
        private readonly RoleManager<IdentityRole> RoleManager;
        private readonly IEmailSender EmailSender;
        private readonly ISmsSender SmsSender;
        private readonly ILogger Logger;
        private IHostingEnvironment _environment;

        public Utility(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext dbContext,
            RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ILoggerFactory loggerFactory,
            IHostingEnvironment environment)
        {
            DB = dbContext;
            UserManager = userManager;
            SignInManager = signInManager;
            EmailSender = emailSender;
            RoleManager = roleManager;
            SmsSender = smsSender;
            _environment = environment;
        }

        public Utility(SignInManager<ApplicationUser> signInManager, ApplicationDbContext dbContext)
        {
            DB = dbContext;
        }

        public string getMonth(int month)
        {
            string[] MonthName = { "มกราคม", "กุมภาพันธ์", "มีนาคม", "เมษายน", "พฤษภาคม", "มิถุนายน", "กรกฎาคม", "สิงหาคม", "กันยายน", "ตุลาคม", "พฤศจิกายน", "ธันวาคม" };
            return MonthName[month - 1];
        }

        public string getMonthShort(int month)
        {
            string[] MonthName = { "ม.ค.", "ก.พ.", "มี.ค.", "เม.ย.", "พ.ค.", "มิ.ย.", "ก.ค.", "ส.ค.", "ก.ย.", "ต.ค.", "พ.ย.", "ธ.ค." };
            return MonthName[month - 1];
        }

        public string getDateTHAndTimeShortMonth(DateTime DateCheck)
        {
            string month = getMonthShort(DateCheck.Month);
            int Year = DateCheck.Year + 543;
            return DateCheck.ToString("dd ") + month + " " + Year.ToString();
        }

        public int getYearTH(DateTime DateCheck)
        {
            int Year = DateCheck.Year + 543;
            return Year;
        }
        
    
    }
}
