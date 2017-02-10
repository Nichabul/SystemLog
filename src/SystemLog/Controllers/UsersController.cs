using System;
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
using SystemLog.Models.AccountViewModels;
using SystemLog.Services;
using SystemLog.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using OfficeOpenXml;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace SystemLog.Controllers
{
    public class UsersController : Controller
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

        public UsersController(
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
        // GET: /<controller>/
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Helper = Helper;
            ViewBag.Company = new SelectList(DB.Companys.ToList(), "CompanyId", "CompanyName");
            var GetUsers = DB.Users.Include(a => a.Roles).Include(b => b.Departments).ToList();
            return View("Index", GetUsers);
        }

        [HttpPost]
        public async Task<IActionResult> Index(int DeptCompanyId, int CompanyId)
        {
            ViewBag.Company = new SelectList(DB.Companys.ToList(), "CompanyId", "CompanyName");
            var GetUsers = await DB.Users.Include(a=>a.Roles).Include(b=>b.Departments).Where(v => v.UserDepartmentsId == DeptCompanyId && v.Departments.Companys.CompanyId == CompanyId).ToListAsync();
            return View("Index", GetUsers);
        }
        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AddUsers(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewBag.Company = new SelectList(DB.Companys.ToList(), "CompanyId", "CompanyName");
            ViewBag.Role = new SelectList(DB.Roles.ToList(), "Name", "Name");

            return View("AddUsers");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUsers(AddUsersViewModel model, string returnUrl , string RoleId)
        {
            ViewData["ReturnUrl"] = returnUrl;
            var currentUser = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Username, FirstName = model.FirstName, LastName = model.LastName, Email = model.Email, UserDepartmentsId = model.UserDepartmentsId };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded) 
                {
                    //Add Role
                    var Roleresult = await _userManager.AddToRoleAsync(user, RoleId);
                    return RedirectToLocal(returnUrl);
                }

                ViewBag.Company = new SelectList(DB.Companys.ToList(), "CompanyId", "CompanyName");
                ViewBag.Department = new SelectList(DB.Departments.ToList(), "DepartmentsId", "DepartmentsName");
                ViewBag.Role = new SelectList(await DB.Roles.ToListAsync(), "Name", "Name");
                AddErrors(result);
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditUsers(string returnUrl, string Id)
        {
            ViewData["ReturnUrl"] = returnUrl;
           

            var user = await DB.Users.Include(a => a.Roles).Include(b => b.Departments).ThenInclude(d=>d.Companys).Where(c => c.Id == Id).FirstOrDefaultAsync();
            var role = await _userManager.GetRolesAsync(user);
            var company = user.Departments.Companys.CompanyId;
            var dept = user.Departments.DepartmentsId;

            ViewBag.Role = new SelectList(DB.Roles.ToList(), "Name", "Name" , role.FirstOrDefault());
            ViewBag.Departments = new SelectList(DB.Departments.Where(a=>a.Companys.CompanyId==company).ToList(), "DepartmentsId", "DepartmentsName", dept);
            ViewBag.Company = new SelectList(DB.Companys.ToList(), "CompanyId", "CompanyName", company);
            return View("EditUsers", user);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUsers(ApplicationUser model, string returnUrl, string RolesUpdate, string OldPassword, string Id)
        {
            ViewData["ReturnUrl"] = returnUrl;
            string msg = "";
            try
            {
                var currentUser = await _userManager.FindByIdAsync(model.Id);
                if (ModelState.IsValid)
                {
                    currentUser.UserName = model.UserName;
                    currentUser.FirstName = model.FirstName;
                    currentUser.LastName = model.LastName;
                    currentUser.Email = model.Email;
                    currentUser.UserDepartmentsId = model.UserDepartmentsId;

                    var result = await _userManager.UpdateAsync(currentUser);
                    var password = await _userManager.ChangePasswordAsync(currentUser, OldPassword, currentUser.PasswordHash);

                    var OldId = await _userManager.FindByIdAsync(currentUser.Id);
                    var OldRoleId = DB.UserRoles.Where(a => a.UserId == OldId.Id).FirstOrDefault();
                    var OldRoleName = DB.Roles.FirstOrDefault(e => e.Id == OldRoleId.RoleId).Name;

                    if (result.Succeeded)
                    {
                        if (OldRoleName != RolesUpdate)
                        {
                            await _userManager.RemoveFromRoleAsync(currentUser, OldRoleName);
                            await _userManager.AddToRoleAsync(currentUser, RolesUpdate);
                        }

                        return RedirectToLocal(returnUrl);
                    }

                    ViewBag.Company = new SelectList(DB.Companys.ToList(), "CompanyId", "CompanyName");
                    ViewBag.Department = new SelectList(DB.Departments.ToList(), "DepartmentsId", "DepartmentsName");
                    ViewBag.Role = new SelectList(await DB.Roles.ToListAsync(), "Name", "Name");
                    AddErrors(result);
                    return RedirectToAction("Index", "Users");
                }
            }catch (Exception e)
            {
                msg = "Error is :" + e.Message;
            }
            
            return View(model);
        }
        // Delete User
        [HttpGet]
        public async Task<IActionResult> DeleteUser(string Id)
        {
            string msg = "";
            try
            {
                var model = await DB.Users.Where(d => d.Id == Id).FirstOrDefaultAsync();
                await _userManager.DeleteAsync(model);
                await DB.SaveChangesAsync();
                msg = "Removed";
            }
            catch(Exception e)
            {
                msg = "Error is :" + e.Message;
            }
             return RedirectToAction("Index", "Users");
        }

        [HttpGet]
        public async Task<IActionResult> DepartmentsLists(int DeptCompanyId)
        {
            string Html = "";
            var model = await DB.Departments.Where(b => b.DeptCompanyId == DeptCompanyId).ToListAsync();
            Html += "<select>";
            Html += "<option value=''>---กรุณเลือก---</option>";
            foreach (var department in model)
            {
                Html += "<option value='" + department.DepartmentsId + "'>" + department.DepartmentsName + "</option>";
            }
            Html += "</select>";
            return Json(Html);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(UsersController.Index), "Users");
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

    }
}
