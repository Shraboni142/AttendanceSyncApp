using AttandanceSyncApp.Controllers.Filters;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Repositories;
using AttandanceSyncApp.Services.Admin;
using AttandanceSyncApp.Services.Interfaces.Admin;
using System;
using System.Web.Mvc;

namespace AttandanceSyncApp.Controllers
{
    /// <summary>
    /// Handles employee management operations for administrators,
    /// including CRUD operations and status management.
    /// </summary>
    [AdminAuthorize]
    public class AdminEmployeesController : BaseController
    {
        /// Employee service for business logic.
        private readonly IEmployeeService _employeeService;

        /// Initializes controller with default services.
        public AdminEmployeesController() : base()
        {
            var unitOfWork = new AuthUnitOfWork();
            _employeeService = new EmployeeService(unitOfWork);
        }

        // GET: AdminEmployees/Index
        public ActionResult Index()
        {
            return View("~/Views/Admin/Employees.cshtml");
        }

        // GET: AdminEmployees/GetEmployees
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

        // GET: AdminEmployees/GetEmployee
        [HttpGet]
        public JsonResult GetEmployee(int id)
        {
            try
            {
                var result = _employeeService.GetEmployeeById(id);

                if (result == null)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = "Employee not found",
                        Errors = new[] { "Employee not found" }
                    });
                }

                if (!result.Success)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = result.Message,
                        Errors = new[] { result.Message }
                    });
                }

                return Json(new
                {
                    Success = true,
                    Data = result.Data
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = new[] { ex.Message }
                });
            }
        }


        // POST: AdminEmployees/CreateEmployee
        [HttpPost]
        public JsonResult CreateEmployee(EmployeeCreateDto dto)
        {
            // ✅ ADD VALIDATION FOR EMAIL
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return Json(ApiResponse.Fail("Employee name is required"));
            }

            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                return Json(ApiResponse.Fail("Employee email is required"));
            }

            // call service
            var result = _employeeService.CreateEmployee(dto);

            if (!result.Success)
            {
                return Json(ApiResponse.Fail(result.Message));
            }

            return Json(ApiResponse.Success(result.Message));
        }

        // POST: AdminEmployees/UpdateEmployee
        [HttpPost]
        public JsonResult UpdateEmployee(EmployeeUpdateDto dto)
        {
            try
            {
                var result = _employeeService.UpdateEmployee(dto);

                return Json(new
                {
                    Success = result.Success,
                    Message = result.Message,
                    Errors = result.Success ? null : new[] { result.Message }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = new[] { ex.Message }
                });
            }
        }


        // POST: AdminEmployees/DeleteEmployee
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

        // POST: AdminEmployees/ToggleEmployeeStatus
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
    }
}
