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

            return View();
        }

        [HttpPost]
public ActionResult Index(UserApproval model)
{
    model.ApprovalStatus = "Pending";
    model.CreatedAt = DateTime.Now;

    db.UserApprovals.Add(model);
    db.SaveChanges();

    var employee = db.Employees.FirstOrDefault(e => e.Id == model.EmployeeId);

    string email = employee != null ? employee.Email : "";

            // ✅ ADD THIS
            var approval = new UserApproval
            {
                EmployeeId = model.EmployeeId,
                EmployeeEmail = email,
                ApprovalStatus = "Pending",
                CreatedAt = DateTime.Now
            };

            db.UserApprovals.Add(approval);
            db.SaveChanges();


            return RedirectToAction("Success");
}

public ActionResult Success()
{
    return Content("Signup submitted. Waiting for admin approval.");
}

    }
}
