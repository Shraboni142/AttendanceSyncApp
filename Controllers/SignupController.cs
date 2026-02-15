using AttandanceSyncApp.Models;
using AttendanceSyncApp.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace AttendanceSyncApp.Controllers
{
    public class SignupController : Controller
    {
        // FIX: Use FULL namespace to avoid ambiguity
        private AttandanceSyncApp.Models.AppDbContext _context = new AttandanceSyncApp.Models.AppDbContext();

        // GET: Signup/Register
        public ActionResult Register()
        {
            ViewBag.Companies = _context.Companies.ToList();
            return View();
        }

        // POST: Signup/Register
        [HttpPost]
        public ActionResult Register(SignupRequest model)
        {
            if (model == null)
            {
                TempData["msg"] = "Invalid request";
                return RedirectToAction("Register");
            }

            model.Status = "Pending";

            _context.SignupRequests.Add(model);
            _context.SaveChanges();

            TempData["msg"] = "Signup Request Sent For Approval";

            return RedirectToAction("Register");
        }
    }
}
