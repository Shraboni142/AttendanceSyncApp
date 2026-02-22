/* ============================
   Admin Employees Management
============================ */
var currentPage = 1;
var pageSize = 20;
var employeeModal = null;

$(function () {

    employeeModal = new bootstrap.Modal(document.getElementById('employeeModal'));

    loadEmployees(1);

    $('#saveEmployeeBtn').on('click', saveEmployee);

});


/* ============================
   Load Employees
============================ */
function loadEmployees(page) {

    currentPage = page;

    $.get(APP.baseUrl + 'AdminEmployees/GetEmployees', {

        page: page,
        pageSize: pageSize

    }, function (res) {

        var tbody = $('#employeesTable tbody');

        tbody.empty();

        if (res.Errors && res.Errors.length > 0) {

            tbody.append('<tr><td colspan="7" class="text-danger">' + res.Message + '</td></tr>');
            return;

        }

        var data = res.Data;

        if (!data.Data || !data.Data.length) {

            tbody.append('<tr><td colspan="7">No employees found</td></tr>');
            return;

        }

        $.each(data.Data, function (_, item) {

            var statusBadge = item.IsActive
                ? '<span class="badge bg-success">Active</span>'
                : '<span class="badge bg-danger">Inactive</span>';

            var actions =
                '<button class="btn btn-sm btn-primary me-1" onclick="editEmployee(' + item.Id + ')">Edit</button>' +

                '<button class="btn btn-sm ' + (item.IsActive ? 'btn-warning' : 'btn-success') + ' me-1" onclick="toggleStatus(' + item.Id + ')">' +

                (item.IsActive ? 'Deactivate' : 'Activate') +

                '</button>' +

                '<button class="btn btn-sm btn-danger" onclick="deleteEmployee(' + item.Id + ')">Delete</button>';

            tbody.append(

                '<tr>' +

                '<td>' + item.Id + '</td>' +

                '<td>' + item.Name + '</td>' +

                '<td>' + item.Email + '</td>' +   // ✅ EMAIL SHOW

                '<td>' + statusBadge + '</td>' +

                '<td>' + formatDateTime(item.CreatedAt) + '</td>' +

                '<td>' + formatDateTime(item.UpdatedAt) + '</td>' +

                '<td>' + actions + '</td>' +

                '</tr>'

            );

        });

        renderPagination(data.TotalRecords, data.Page, data.PageSize);

    });

}


/* ============================
   Show Create Modal
============================ */
function showCreateModal() {

    $('#employeeModalTitle').text('Add Employee');

    $('#employeeId').val('');

    $('#employeeName').val('');

    $('#employeeEmail').val('');   // ✅ CLEAR EMAIL

    $('#employeeActive').prop('checked', true);

    employeeModal.show();

}


/* ============================
   Edit Employee
============================ */
function editEmployee(id) {

    $.get(APP.baseUrl + 'AdminEmployees/GetEmployee', { id: id })

        .done(function (res) {

            console.log(res); // optional debug

            if (!res || res.Success === false) {

                Swal.fire('Error', res.Message || 'Failed to load employee', 'error');

                return;

            }

            var employee = res.Data;

            if (!employee) {

                Swal.fire('Error', 'Employee data not found', 'error');

                return;

            }

            $('#employeeModalTitle').text('Edit Employee');

            $('#employeeId').val(employee.Id);

            $('#employeeName').val(employee.Name || '');

            $('#employeeEmail').val(employee.Email || '');

            $('#employeeActive').prop('checked', employee.IsActive);

            employeeModal.show();

        })

        .fail(function () {

            Swal.fire('Error', 'Server error while loading employee', 'error');

        });

}




/* ============================
   Save Employee
============================ */
function saveEmployee() {

    var id = $('#employeeId').val();

    var name = $('#employeeName').val().trim();

    var email = $('#employeeEmail').val().trim();   // ✅ GET EMAIL

    var isActive = $('#employeeActive').prop('checked');

    if (!name) {

        Swal.fire('Validation Error', 'Employee name is required', 'warning');

        return;

    }

    if (!email) {

        Swal.fire('Validation Error', 'Employee email is required', 'warning');

        return;

    }

    var url = id
        ? APP.baseUrl + 'AdminEmployees/UpdateEmployee'
        : APP.baseUrl + 'AdminEmployees/CreateEmployee';

    var data = {

        Id: id ? parseInt(id) : 0,

        Name: name,

        Email: email,   // ✅ SEND EMAIL

        IsActive: isActive

    };

    $.ajax({

        url: url,

        type: 'POST',

        data: data,

        success: function (res) {

            if (res.Errors && res.Errors.length > 0) {

                Swal.fire('Error', res.Message, 'error');

            }
            else {

                Swal.fire('Success', res.Message, 'success');

                employeeModal.hide();

                loadEmployees(currentPage);

            }

        },

        error: function () {

            Swal.fire('Error', 'Failed to save employee', 'error');

        }

    });

}


/* ============================
   Toggle Status
============================ */
function toggleStatus(id) {

    Swal.fire({

        title: 'Confirm',

        text: 'Are you sure you want to change this employee\'s status?',

        icon: 'question',

        showCancelButton: true,

        confirmButtonColor: '#3085d6',

        cancelButtonColor: '#d33',

        confirmButtonText: 'Yes'

    }).then((result) => {

        if (result.isConfirmed) {

            $.post(APP.baseUrl + 'AdminEmployees/ToggleEmployeeStatus',

                { id: id },

                function () {

                    loadEmployees(currentPage);

                });

        }

    });

}


/* ============================
   Delete Employee
============================ */
function deleteEmployee(id) {

    Swal.fire({

        title: 'Delete Employee',

        icon: 'warning',

        showCancelButton: true,

        confirmButtonText: 'Delete'

    }).then((result) => {

        if (result.isConfirmed) {

            $.post(APP.baseUrl + 'AdminEmployees/DeleteEmployee',

                { id: id },

                function () {

                    loadEmployees(currentPage);

                });

        }

    });

}


/* ============================
   Pagination
============================ */
function renderPagination(totalRecords, page, pageSize) {

    var totalPages = Math.ceil(totalRecords / pageSize);

    var pagination = $('#pagination');

    pagination.empty();

    for (var i = 1; i <= totalPages; i++) {

        pagination.append(

            '<button class="btn btn-sm btn-outline-primary me-1 ' +

            (i === page ? 'active' : '') +

            '" onclick="loadEmployees(' + i + ')">' +

            i +

            '</button>'

        );

    }

}
