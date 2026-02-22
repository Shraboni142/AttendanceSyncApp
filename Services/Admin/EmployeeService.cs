using System;
using System.Linq;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Models.DTOs.Admin;
using AttandanceSyncApp.Models.AttandanceSync;
using AttandanceSyncApp.Repositories.Interfaces;
using AttandanceSyncApp.Services.Interfaces.Admin;

namespace AttandanceSyncApp.Services.Admin
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IAuthUnitOfWork _unitOfWork;

        public EmployeeService(IAuthUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ✅ GET PAGED EMPLOYEES (Email added)
        public ServiceResult<PagedResultDto<EmployeeDto>> GetEmployeesPaged(int page, int pageSize)
        {
            try
            {
                var totalCount = _unitOfWork.Employees.Count();

                var employees = _unitOfWork.Employees.GetAll()
                    .OrderByDescending(e => e.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    Name = e.Name,

                    Email = e.Email ?? "",   // ✅ FIX

                    IsActive = e.IsActive,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt
                })

                    .ToList();

                var result = new PagedResultDto<EmployeeDto>
                {
                    TotalRecords = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    Data = employees
                };

                return ServiceResult<PagedResultDto<EmployeeDto>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<PagedResultDto<EmployeeDto>>
                    .FailureResult($"Failed to retrieve employees: {ex.Message}");
            }
        }

        // ✅ GET BY ID (Email added)
        public ServiceResult<EmployeeDto> GetEmployeeById(int id)
        {
            try
            {
                var employee = _unitOfWork.Employees.GetById(id);

                if (employee == null)
                {
                    return ServiceResult<EmployeeDto>.FailureResult("Employee not found");
                }

                var dto = new EmployeeDto
                {
                    Id = employee.Id,

                    Name = employee.Name,

                    Email = employee.Email ?? "",   // ✅ FIX: prevents crash if NULL

                    IsActive = employee.IsActive,

                    CreatedAt = employee.CreatedAt,

                    UpdatedAt = employee.UpdatedAt
                };


                return ServiceResult<EmployeeDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<EmployeeDto>
                    .FailureResult($"Failed to retrieve employee: {ex.Message}");
            }
        }

        // ✅ CREATE EMPLOYEE (Email added)
        public ServiceResult CreateEmployee(EmployeeCreateDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    return ServiceResult.FailureResult("Employee name is required");
                }

                if (string.IsNullOrWhiteSpace(dto.Email))
                {
                    return ServiceResult.FailureResult("Employee email is required");
                }

                var employee = new Employee
                {
                    Name = dto.Name.Trim(),
                    Email = dto.Email?.Trim() ?? "",

                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.Now
                };

                _unitOfWork.Employees.Add(employee);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Employee created");
            }
            catch (Exception ex)
            {
                return ServiceResult
                    .FailureResult($"Failed to create employee: {ex.Message}");
            }
        }

        // ✅ UPDATE EMPLOYEE (Email added)
        public ServiceResult UpdateEmployee(EmployeeUpdateDto dto)
        {
            try
            {
                var employee = _unitOfWork.Employees.GetById(dto.Id);

                if (employee == null)
                {
                    return ServiceResult.FailureResult("Employee not found");
                }

                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    return ServiceResult.FailureResult("Employee name is required");
                }

                if (string.IsNullOrWhiteSpace(dto.Email))
                {
                    return ServiceResult.FailureResult("Employee email is required");
                }

                employee.Name = dto.Name.Trim();

                employee.Email = dto.Email?.Trim() ?? "";


                employee.IsActive = dto.IsActive;
                employee.UpdatedAt = DateTime.Now;

                _unitOfWork.Employees.Update(employee);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Employee updated");
            }
            catch (Exception ex)
            {
                return ServiceResult
                    .FailureResult($"Failed to update employee: {ex.Message}");
            }
        }

        // DELETE EMPLOYEE
        public ServiceResult DeleteEmployee(int id)
        {
            try
            {
                var employee = _unitOfWork.Employees.GetById(id);

                if (employee == null)
                {
                    return ServiceResult.FailureResult("Employee not found");
                }

                var hasRequests = _unitOfWork.AttandanceSyncRequests
                    .Count(r => r.EmployeeId == id) > 0;

                if (hasRequests)
                {
                    return ServiceResult
                        .FailureResult("Cannot delete employee with existing requests");
                }

                _unitOfWork.Employees.Remove(employee);
                _unitOfWork.SaveChanges();

                return ServiceResult.SuccessResult("Employee deleted");
            }
            catch (Exception ex)
            {
                return ServiceResult
                    .FailureResult($"Failed to delete employee: {ex.Message}");
            }
        }

        // TOGGLE STATUS
        public ServiceResult ToggleEmployeeStatus(int id)
        {
            try
            {
                var employee = _unitOfWork.Employees.GetById(id);

                if (employee == null)
                {
                    return ServiceResult.FailureResult("Employee not found");
                }

                employee.IsActive = !employee.IsActive;
                employee.UpdatedAt = DateTime.Now;

                _unitOfWork.Employees.Update(employee);
                _unitOfWork.SaveChanges();

                var status = employee.IsActive ? "activated" : "deactivated";

                return ServiceResult.SuccessResult($"Employee {status}");
            }
            catch (Exception ex)
            {
                return ServiceResult
                    .FailureResult($"Failed to toggle employee status: {ex.Message}");
            }
        }
    }
}
