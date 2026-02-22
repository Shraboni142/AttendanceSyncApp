using System.Linq;
using System.Web.Mvc;
using AttandanceSyncApp.Models;

namespace AttandanceSyncApp.Controllers
{
    public class AdminController : Controller
    {
        public ActionResult Dashboard()
        {
            using (var db = new AppDbContext())
            {
                ViewBag.PendingRequestsCount =
                    db.SyncRequests.Count(r => r.Status == "Pending");

                ViewBag.RecentRequests =
                    db.SyncRequests
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(10)
                    .ToList();
            }

            return View();
        }
    }
}
