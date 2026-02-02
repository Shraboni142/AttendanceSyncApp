/* ============================
   Admin Sync Requests Management
============================ */
var currentPage = 1;
var pageSize = 20;

$(function () {
    loadRequests(1);

    // Auto-refresh every 2 seconds
    setInterval(function () {
        loadRequests(currentPage);
    }, 2000);
});

function loadRequests(page) {
    currentPage = page;

    var filter = {
        Page: page,
        PageSize: pageSize
    };

    $.get(APP.baseUrl + 'AdminRequests/GetAllRequests', filter, function (res) {
        var tbody = $('#requestsTable tbody').empty();

        if (res.Errors && res.Errors.length > 0) {
            tbody.append('<tr><td colspan="12" class="text-danger text-center">' + res.Message + '</td></tr>');
            $('#totalRequestsCount').text('0 Requests');
            return;
        }

        var data = res.Data;
        if (!data.Data || !data.Data.length) {
            tbody.append('<tr><td colspan="12" class="text-center">No requests found</td></tr>');
            $('#totalRequestsCount').text('0 Requests');
            $('#pagination').empty();
            return;
        }

        // Update total count
        $('#totalRequestsCount').text(
            data.TotalRecords + ' Request' + (data.TotalRecords !== 1 ? 's' : '')
        );

        $.each(data.Data, function (_, item) {
            var statusBadge = getStatusBadgeFromBoolean(item.IsSuccessful);
            var externalIdDisplay = item.ExternalSyncId
                ? item.ExternalSyncId
                : '<span class="text-muted">-</span>';

            tbody.append(
                '<tr>' +
                '<td>' + item.Id + '</td>' +
                '<td>' + escapeHtml(item.UserName) +
                '<br><small class="text-muted">' + escapeHtml(item.UserEmail) + '</small></td>' +
                '<td>' + escapeHtml(item.EmployeeName) + '</td>' +
                '<td>' + escapeHtml(item.CompanyName) + '</td>' +
                '<td>' + escapeHtml(item.ToolName) + '</td>' +
                '<td>' + formatDate(item.FromDate) + '</td>' +
                '<td>' + formatDate(item.ToDate) + '</td>' +
                '<td>' + externalIdDisplay + '</td>' +
                '<td>' + statusBadge + '</td>' +
                '<td>' + formatDateTime(item.CreatedAt) + '</td>' +
                '<td>' + formatDateTime(item.UpdatedAt) + '</td>' +
                '<td>' +
                '<button class="btn btn-sm btn-info me-1" onclick="viewDetails(' + item.Id + ')">' +
                '<i class="bi bi-eye"></i> View</button>' +
                '<button class="btn btn-sm btn-success btn-approve me-1" data-id="' + item.Id + '">' +
                'Approve</button>' +
                '<button class="btn btn-sm btn-danger btn-reject" data-id="' + item.Id + '">' +
                'Reject</button>' +
                '</td>' +
                '</tr>'
            );
        });

        renderPagination('#pagination', data.TotalRecords, data.Page, data.PageSize, loadRequests);

    }).fail(function (xhr) {
        var tbody = $('#requestsTable tbody');
        if (xhr.status === 401) {
            tbody.html('<tr><td colspan="12" class="text-center text-danger">Session expired. Please login again.</td></tr>');
        } else {
            tbody.html('<tr><td colspan="12" class="text-center text-danger">Failed to load requests</td></tr>');
        }
        $('#totalRequestsCount').text('0 Requests');
    });
}

function viewDetails(requestId) {
    $.get(APP.baseUrl + 'AdminRequests/GetRequest?id=' + requestId, function (res) {
        if (res.Errors && res.Errors.length > 0) {
            Swal.fire('Error', res.Message, 'error');
            return;
        }

        var request = res.Data;

        $('#detailRequestId').text(request.Id);
        $('#detailId').text(request.Id);
        $('#detailStatus').html(getStatusBadgeFromBoolean(request.IsSuccessful));
        $('#detailFromDate').text(formatDate(request.FromDate));
        $('#detailToDate').text(formatDate(request.ToDate));
        $('#detailExternalSyncId').text(request.ExternalSyncId || '-');
        $('#detailCreatedAt').text(formatDateTime(request.CreatedAt));
        $('#detailUpdatedAt').text(formatDateTime(request.UpdatedAt));

        $('#detailUserId').text(request.UserId);
        $('#detailUserName').text(request.UserName);
        $('#detailUserEmail').text(request.UserEmail);

        $('#detailEmployeeId').text(request.EmployeeId);
        $('#detailEmployeeName').text(request.EmployeeName);

        $('#detailCompanyId').text(request.CompanyId);
        $('#detailCompanyName').text(request.CompanyName);
        $('#detailToolId').text(request.ToolId);
        $('#detailToolName').text(request.ToolName);

        $('#detailSessionId').text(request.SessionId || '-');
        loadSessionInfo(request.SessionId);

        var modal = new bootstrap.Modal(
            document.getElementById('requestDetailsModal')
        );
        modal.show();
    }).fail(function () {
        Swal.fire('Error', 'Failed to load request details', 'error');
    });
}

function loadSessionInfo(sessionId) {
    if (!sessionId) {
        $('#sessionInfoContainer').html('<p class="text-muted">No session information available</p>');
        return;
    }

    $.get(APP.baseUrl + 'AdminSessions/GetSession?id=' + sessionId, function (res) {
        if (res.Errors && res.Errors.length > 0) {
            $('#sessionInfoContainer').html('<p class="text-danger">Failed to load session information</p>');
            return;
        }

        var session = res.Data;
        var sessionHtml =
            '<div class="row g-2">' +
            '<div class="col-md-6 d-flex gap-2"><strong>Login Time:</strong><p>' +
            formatDateTime(session.LoginTime) + '</p></div>' +
            '<div class="col-md-6 d-flex gap-2"><strong>Logout Time:</strong><p>' +
            formatDateTime(session.LogoutTime) + '</p></div>' +
            '<div class="col-md-6 d-flex gap-2"><strong>IP Address:</strong><p>' +
            escapeHtml(session.IpAddress || '-') + '</p></div>' +
            '<div class="col-md-6 d-flex gap-2"><strong>User Agent:</strong><p>' +
            escapeHtml(session.UserAgent || '-') + '</p></div>' +
            '<div class="col-md-6 d-flex gap-2"><strong>Is Active:</strong><p>' +
            (session.IsActive
                ? '<span class="badge bg-success">Active</span>'
                : '<span class="badge bg-secondary">Inactive</span>') +
            '</p></div>' +
            '</div>';

        $('#sessionInfoContainer').html(sessionHtml);
    }).fail(function () {
        $('#sessionInfoContainer').html('<p class="text-danger">Failed to load session information</p>');
    });
}

function getStatusBadgeFromBoolean(isSuccessful) {
    if (isSuccessful === true) return '<span class="badge bg-success">Completed</span>';
    if (isSuccessful === false) return '<span class="badge bg-danger">Failed</span>';
    return '<span class="badge bg-warning text-dark">Pending</span>';
}

/* ==================================================
   ADD-ONLY: Approve / Reject Button Click Handlers
================================================== */

$(document).on('click', '.btn-approve', function () {
    var requestId = $(this).data('id');

    if (!confirm('Are you sure you want to approve this request?')) {
        return;
    }

    $.post(APP.baseUrl + 'AdminRequests/UpdateRequestStatus', {
        requestId: requestId,
        status: 'Approved'
    })
        .done(function (res) {
            if (res.Success) {
                alert('Request approved successfully');
                loadRequests(currentPage);
            } else {
                alert(res.Message || 'Approval failed');
            }
        })
        .fail(function () {
            alert('Server error while approving request');
        });
});

$(document).on('click', '.btn-reject', function () {
    var requestId = $(this).data('id');

    if (!confirm('Are you sure you want to reject this request?')) {
        return;
    }

    $.post(APP.baseUrl + 'AdminRequests/UpdateRequestStatus', {
        requestId: requestId,
        status: 'Rejected'
    })
        .done(function (res) {
            if (res.Success) {
                alert('Request rejected');
                loadRequests(currentPage);
            } else {
                alert(res.Message || 'Rejection failed');
            }
        })
        .fail(function () {
            alert('Server error while rejecting request');
        });
});
