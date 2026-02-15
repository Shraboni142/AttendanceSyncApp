using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Services.Admin;
using AttandanceSyncApp.Services.Interfaces.Admin;
using System.Web.Mvc;
using AttandanceSyncApp.Models.AttandanceSync;

namespace AttandanceSyncApp.Controllers
{
    [AdminAuthorize]
    public class AdminEmployeesController : BaseController
    {
        private readonly IEmployeeService _employeeService;

        public AdminEmployeesController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            _employeeService = new EmployeeService(unitOfWork);
        }

        // ================= INDEX VIEW =================
        public ActionResult Index()
        {
            return View("~/Views/Admin/Employees.cshtml");
        }

        // =============== GET ALL EMPLOYEES ===============
        [HttpGet]
        public JsonResult GetEmployees(int page = 1, int pageSize = 20)
        {
            var result = _employeeService.GetEmployeesPaged(page, pageSize);

            if (!result.Success)
            {
                return Json(ApiResponse<object>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<object>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // =============== GET SINGLE EMPLOYEE ===============
        [HttpGet]
        public JsonResult GetEmployee(int id)
        {
            var result = _employeeService.GetEmployeeById(id);

            if (!result.Success)
            {
                return Json(ApiResponse<EmployeeDto>.Fail(result.Message), JsonRequestBehavior.AllowGet);
            }

            return Json(ApiResponse<EmployeeDto>.Success(result.Data), JsonRequestBehavior.AllowGet);
        }

        // =============== CREATE EMPLOYEE (DTO WAY) ===============
        [HttpPost]
        public JsonResult CreateEmployee(EmployeeCreateDto dto)
        {
            // 🔥 EMAIL ADD SUPPORT THROUGH DTO
            var result = _employeeService.CreateEmployee(dto);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // =============== UPDATE EMPLOYEE ===============
        [HttpPost]
        public JsonResult UpdateEmployee(EmployeeUpdateDto dto)
        {
            // 🔥 EMAIL ADD SUPPORT THROUGH DTO
            var result = _employeeService.UpdateEmployee(dto);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // =============== DELETE EMPLOYEE ===============
        [HttpPost]
        public JsonResult DeleteEmployee(int id)
        {
            var result = _employeeService.DeleteEmployee(id);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // =============== TOGGLE STATUS ===============
        [HttpPost]
        public JsonResult ToggleEmployeeStatus(int id)
        {
            var result = _employeeService.ToggleEmployeeStatus(id);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // ======================================================
        // ✅ SIMPLE CREATE METHOD (AS YOU ASKED)
        // ======================================================
        [HttpPost]
        public ActionResult Create(Employee model)
        {
            using (var db = new AuthDbContext())
            {
                // 🔥 NEW EMAIL SUPPORT ADDED
                var employee = new Employee();

                employee.Name = model.Name;
                employee.Email = model.Email;      // NEW LINE
                employee.IsActive = model.IsActive;

                db.Employees.Add(employee);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
