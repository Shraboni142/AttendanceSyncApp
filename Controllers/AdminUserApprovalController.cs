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

            // DEBUG LINE
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

            if (record == null)
                return Json(new { success = false, message = "Not found" });

            // 1. Mark Approved
            record.IsAdminApproved = true;

            // 2. Auto Assign Default Tool (ToolId = 1)
            var already = _uow.UserTools
                .GetAll()
                .Any(x => x.UserId == record.UserId && x.ToolId == 1);

            if (!already)
            {
                var ut = new UserTool
                {
                    UserId = record.UserId,
                    ToolId = 1,
                    AssignedBy = CurrentUser.Id,
                    AssignedAt = DateTime.Now,
                    IsRevoked = false
                };

                _uow.UserTools.Add(ut);
            }

            _uow.SaveChanges();

            return Json(new { success = true });
        }


    }
}
