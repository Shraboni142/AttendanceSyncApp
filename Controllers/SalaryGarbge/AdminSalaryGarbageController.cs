using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Repositories.Interfaces;
using System.Linq;
using System.Web.Mvc;

namespace AttandanceSyncApp.Controllers.SalaryGarbage
{
    public class AdminSalaryGarbageController : Controller
    {
        private readonly IAuthUnitOfWork _unitOfWork;

        // Constructor
        public AdminSalaryGarbageController()
        {
            _unitOfWork = new AuthUnitOfWork();
        }

        // GET: AdminSalaryGarbage/Index
        public ActionResult Index()
        {
            ViewBag.ActiveMenu = "SalaryGarbage";

            var data = _unitOfWork.Context.SalaryGarbageRecords
                            .OrderByDescending(x => x.EntryDateTime)
                            .ToList();

            return View(data);
        }

        // POST: Clear old data
        [HttpPost]
        public JsonResult ClearOldGarbage()
        {
            _unitOfWork.Context.SalaryGarbageRecords.RemoveRange(
                _unitOfWork.Context.SalaryGarbageRecords
            );

            _unitOfWork.Context.SaveChanges();

            return Json(new { success = true });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWork?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}