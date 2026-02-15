using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Repositories;
using System;
using System.Linq;
using System.Web.Mvc;
using AttandanceSyncApp.Models.AttandanceSync;

namespace AttandanceSyncApp.Controllers
{
    [AdminAuthorize]
    public class AdminUserApprovalController : BaseController
    {
        private readonly AuthUnitOfWork _uow;

        public AdminUserApprovalController()
        {
            _uow = new AuthUnitOfWork();
        }

        public ActionResult Index()
        {
            ViewBag.ActiveMenu = "UserApprovals";

            var pending = _uow.UserApprovalStatus
                .GetAll()
                .Where(x => x.IsAdminApproved == false)
                .ToList();

            System.Diagnostics.Debug.WriteLine("TOTAL PENDING = " + pending.Count);

            return View(pending);
        }

        [HttpGet]
        public JsonResult GetPendingUsers()
        {
            var list = _uow.UserApprovalStatus
                .GetAll()
                .Where(x => x.IsAdminApproved == false)
                .Select(x => new {
                    x.Id,
                    x.UserId,
                    x.IsEmailVerified,
                    x.IsAdminApproved
                })
                .ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ApproveUser(int id)
        {
            var record = _uow.UserApprovalStatus
                .GetAll()
                .FirstOrDefault(x => x.Id == id);

            var db = new AppDbContext();

            var signup = db.SignupRequests
                            .FirstOrDefault(s => s.Id == id);

            if (record == null || signup == null)
                return Json(new { success = false, message = "Not found" });

            var authDb = new AuthDbContext();

            var employee = authDb.Employees
                .FirstOrDefault(e => e.Id == signup.EmployeeId);

            if (employee == null)
            {
                return Json(new { success = false, message = "Employee not found" });
            }

            if (employee.Email != signup.Email)
            {
                return Json(new { success = false, message = "Email Not Matching" });
            }

            signup.Status = "Approved";
            record.IsAdminApproved = true;

            var already = _uow.UserTools
                .GetAll()
                .Any(x => x.UserId == record.UserId
                       && x.ToolId == signup.ToolId);

            if (!already)
            {
                var ut = new UserTool
                {
                    UserId = record.UserId,

                    // ✅ FIXED LINE (ONLY THIS WAS WRONG)
                    ToolId = signup.ToolId ?? 0,

                    AssignedBy = CurrentUser.Id,
                    AssignedAt = DateTime.Now,
                    IsRevoked = false
                };

                _uow.UserTools.Add(ut);
            }

            _uow.SaveChanges();
            db.SaveChanges();

            return Json(new { success = true, message = "Approved Successfully" });
        }
    }
}
