using AttandanceSyncApp.Models;
using AttendanceSyncApp.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace AttendanceSyncApp.Controllers
{
    public class SignupController : Controller
    {
        private AppDbContext db = new AppDbContext();

        public ActionResult Index()
        {
            ViewBag.Employees = db.Employees.ToList();
            ViewBag.Companies = db.Companies.ToList();
            ViewBag.Tools = db.Tools.ToList();

            return View("~/Views/Auth/Register.cshtml");
        }

        [HttpPost]
        public ActionResult Index(UserApproval model)
        {
            // 🔒 STRICT DUPLICATE CHECK
            var alreadyExists = db.UserApprovals
                .Any(x =>
                    x.EmployeeId == model.EmployeeId &&
                    x.CompanyId == model.CompanyId &&
                    x.ToolId == model.ToolId
                );

            if (alreadyExists)
            {
                ModelState.AddModelError("",
                    "Signup failed. This employee already has a request for this company and tool.");

                ViewBag.Employees = db.Employees.ToList();
                ViewBag.Companies = db.Companies.ToList();
                ViewBag.Tools = db.Tools.ToList();

                return View("~/Views/Auth/Register.cshtml", model);
            }

            // ✅ Single Insert Only
            var employee = db.Employees.FirstOrDefault(e => e.Id == model.EmployeeId);

            model.EmployeeEmail = employee != null ? employee.Email : "";
            model.ApprovalStatus = "Pending";
            model.CreatedAt = DateTime.Now;

            db.UserApprovals.Add(model);
            db.SaveChanges();

            return RedirectToAction("Success");
        }

        public ActionResult Success()
        {
            return Content("Signup submitted. Waiting for admin approval.");
        }
    }
}