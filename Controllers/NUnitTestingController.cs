using System.IO;
using System.Web;
using System.Web.Mvc;
using AttendanceSyncApp.Services.NUnit;

namespace AttendanceSyncApp.Controllers
{
    [Authorize(Roles = "ADMIN,USER")]
    public class NUnitTestingController : Controller
    {
        // GET: /NUnitTesting/Index
        public ActionResult Index()
        {
            return View();
        }

        // GET: /NUnitTesting/Run
        public ActionResult Run()
        {
            return View();
        }

        // POST: /NUnitTesting/Run
        [HttpPost]
        public ActionResult Run(HttpPostedFileBase testDll)
        {
            // 1️⃣ DLL uploaded কিনা check
            if (testDll == null || testDll.ContentLength == 0)
            {
                ViewBag.ResultXml = "❌ Please upload a valid NUnit test DLL.";
                return View();
            }

            // 2️⃣ App_Data/TestDlls folder তৈরি
            var folderPath = Server.MapPath("~/App_Data/TestDlls/");
            Directory.CreateDirectory(folderPath);

            // 3️⃣ DLL save করা
            var fullPath = Path.Combine(folderPath, Path.GetFileName(testDll.FileName));
            testDll.SaveAs(fullPath);

            // 4️⃣ NUnit Console Runner call
            var runner = new NUnitTestRunnerService();
            var result = runner.Run(fullPath);

            // 5️⃣ Result View এ পাঠানো
            ViewBag.ResultXml = result;
            // ---------- Simple result parsing ----------
int total = 0, passed = 0, failed = 0;

// NUnit console output থেকে simple count
if (!string.IsNullOrEmpty(result))
{
    foreach (var line in result.Split('\n'))
    {
        if (line.Contains("Test Count:"))
        {
            // Example: Test Count: 5, Passed: 4, Failed: 1, Skipped: 0
            try
            {
                var parts = line.Split(',');
                total = int.Parse(parts[0].Split(':')[1].Trim());
                passed = int.Parse(parts[1].Split(':')[1].Trim());
                failed = int.Parse(parts[2].Split(':')[1].Trim());
            }
            catch { }
        }
    }
}

ViewBag.TotalTests = total;
ViewBag.PassedTests = passed;
ViewBag.FailedTests = failed;


            return View();
        }
    }
}
