using System.Linq;
using System.Web.Mvc;
using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Repositories;

namespace AttandanceSyncApp.Controllers
{
    [AdminAuthorize]
    public class AdminDashboardController : BaseController
    {
        public AdminDashboardController() : base()
        {
        }

        // GET: AdminDashboard/Index
        // GET: AdminDashboard/Index
        public ActionResult Index()
        {
            using (var unitOfWork = new AuthUnitOfWork())
            {
                // ✅ Total Requests
                var totalRequests = unitOfWork.AttandanceSyncRequests
                    .GetAll()
                    .Count();

                // ✅ Pending Requests (IsSuccessful == null)
                var pendingRequests = unitOfWork.AttandanceSyncRequests
                    .GetAll()
                    .Count(r => r.IsSuccessful == null);

                // ✅ Completed Requests (IsSuccessful == true)
                var completedRequests = unitOfWork.AttandanceSyncRequests
                    .GetAll()
                    .Count(r => r.IsSuccessful == true);

                // ✅ Recent Requests (Top 10)
                var recentRequests = unitOfWork.AttandanceSyncRequests
                    .GetAllWithDetails()
                    .OrderByDescending(r => r.Id)
                    .Take(10)
                    .Select(r => new
                    {
                        Id = r.Id,
                        Email = r.Employee != null ? r.Employee.Email : "",
                        CompanyId = r.CompanyId,
                        Status = r.IsSuccessful == null ? "Pending"
                               : r.IsSuccessful == true ? "Completed"
                               : "Failed"
                    })
                    .ToList();

                // ✅ Send to View
                ViewBag.TotalRequests = totalRequests;
                ViewBag.PendingRequestsCount = pendingRequests;
                ViewBag.CompletedRequestsCount = completedRequests;
                ViewBag.RecentRequests = recentRequests;
            }

            return View("~/Views/Admin/Dashboard.cshtml");
        }








        // EXISTING METHOD (NO CHANGE)
        [HttpGet]
        public JsonResult GetStats()
        {
            using (var unitOfWork = new AuthUnitOfWork())
            {
                var totalUsers = unitOfWork.Users.Count();
                var totalRequests = unitOfWork.AttandanceSyncRequests.GetTotalCount();
                var totalEmployees = unitOfWork.Employees.Count();
                var totalCompanies = unitOfWork.SyncCompanies.Count();
                var totalTools = unitOfWork.Tools.Count();

                var stats = new
                {
                    TotalUsers = totalUsers,
                    TotalRequests = totalRequests,
                    TotalEmployees = totalEmployees,
                    TotalCompanies = totalCompanies,
                    TotalTools = totalTools
                };

                return Json(ApiResponse<object>.Success(stats), JsonRequestBehavior.AllowGet);
            }
        }

    }
}
