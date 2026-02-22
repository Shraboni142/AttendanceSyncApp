using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.Auth;
using AttendanceSyncApp.Models;
using System;
using System.Linq;
using System.Web.Mvc;

public class AdminUserApprovalController : Controller
{
    private AppDbContext db = new AppDbContext();

    public ActionResult Index()
    {
        var list = db.UserApprovals.ToList();
        return View(list);
    }

    public ActionResult Approve(int id)
    {
        var approval = db.UserApprovals.Find(id);

        if (approval == null)
            return RedirectToAction("Index");


        // update approval status
        approval.ApprovalStatus = "Approved";


        // check existing user by EMAIL ONLY
        var existingUser = db.Users
            .FirstOrDefault(x => x.Email == approval.EmployeeEmail);


        if (existingUser == null)
        {
            var newUser = new User
            {
                Name = approval.EmployeeEmail,

                Email = approval.EmployeeEmail,

                Role = "USER",

                IsActive = true,

                CreatedAt = DateTime.Now,

                UpdatedAt = DateTime.Now

                // NO PASSWORD REQUIRED
            };

            db.Users.Add(newUser);
        }

        db.SaveChanges();

        return RedirectToAction("Index");
    }
}
