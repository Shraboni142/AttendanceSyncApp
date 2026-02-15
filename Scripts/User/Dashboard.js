/* ==============================
   User Dashboard - Tool Cards
   (ORIGINAL UI - FIXED)
============================== */

$(function () {
    loadUserTools(); // ONLY this

    // ===== NEW: AUTO REFRESH EVERY 10 SECONDS =====
    startAutoRefresh();

    // ===== NEW: REAL TIME APPROVAL POPUP =====
    startApprovalListener();
});

function startAutoRefresh() {
    setInterval(function () {
        loadUserTools();
    }, 10000); // 10 seconds
}

function loadUserTools() {

    $.get(APP.baseUrl + 'Attandance/GetMyTools', function (res) {

        var container = $('#toolsContainer').empty();

        if (!res || res.Success === false) {
            container.html(
                '<div class="col-12 text-center text-danger">' +
                (res && res.Message ? res.Message : 'Failed to load tools') +
                '</div>'
            );
            return;
        }

        if (!res.Data || res.Data.length === 0) {
            container.addClass('d-none');
            $('#noToolsMessage').removeClass('d-none');
            return;
        }

        $('#noToolsMessage').addClass('d-none');
        container.removeClass('d-none');

        // ===== NEW: SHOW PENDING BADGE =====
        $.get(APP.baseUrl + 'CompanyRequest/GetMyRequests', function (r) {

            if (r && r.Data && r.Data.Items) {

                var pending = r.Data.Items
                    .filter(x => x.Status === "Pending").length;

                $('#pendingBadge').remove();

                if (pending > 0) {
                    $('.navbar').append(
                        '<span id="pendingBadge" class="badge bg-danger ms-2">'
                        + pending + ' Pending</span>'
                    );
                }
            }

        });
        // ===== END NEW =====


        $.each(res.Data, function (_, tool) {

            var cardClass = tool.IsImplemented
                ? 'implemented'
                : 'under-development';

            var icon = getToolIcon(tool.ToolName);

            var badge = tool.IsImplemented
                ? ''
                : '<span class="badge-development">Coming Soon</span>';

            var card =
                '<div class="col-md-4 col-lg-3 mb-4">' +
                '<div class="card tool-card shadow ' + cardClass + '"' +
                ' data-route="' + (tool.RouteUrl || '') + '"' +
                ' data-implemented="' + tool.IsImplemented + '"' +
                ' data-tool-name="' + escapeHtml(tool.ToolName) + '">' +
                badge +
                '<div class="card-body">' +
                '<div class="tool-icon">' + icon + '</div>' +
                '<div class="tool-name">' + escapeHtml(tool.ToolName) + '</div>' +
                '<div class="tool-description">' +
                escapeHtml(tool.ToolDescription || '') +
                '</div>' +
                '</div>' +
                '</div>' +
                '</div>';

            container.append(card);
        });

        $('.tool-card').on('click', function () {

            var isImplemented = $(this).data('implemented');
            var route = $(this).data('route');
            var toolName = $(this).data('tool-name');

            // ===== ACCESS CHECK =====

            $.get(APP.baseUrl + 'Attandance/CheckToolAccess',
                { toolName: toolName },
                function (res) {

                    if (!res.hasAccess) {
                        window.location.href = APP.baseUrl + 'CompanyRequest/Index';
                        return;
                    }

                    // ✔ SAVE SELECTED TOOL
                    localStorage.setItem("selectedTool", toolName);

                    if (isImplemented && route) {
                        window.location.href = route.replace('~/', APP.baseUrl);
                    }
                    else {
                        showUnderDevelopmentModal(toolName);
                    }

                });

        });

    }).fail(function (xhr) {

        if (xhr.status === 401) {
            window.location.href = APP.baseUrl + 'Auth/Login';
            return;
        }

        $('#toolsContainer').html(
            '<div class="col-12 text-center text-danger">Failed to load tools</div>'
        );
    });
}

/* ==============================
   REAL TIME APPROVAL POPUP
============================== */

function startApprovalListener() {

    setInterval(function () {

        $.get(APP.baseUrl + 'Attandance/GetMyTools', function (res) {

            if (!res.Data) return;

            var currentCount = $('.tool-card').length;
            var newCount = res.Data.length;

            if (newCount > currentCount) {

                Swal.fire({
                    icon: 'success',
                    title: 'Access Granted!',
                    text: 'New tool has been approved for you',
                    confirmButtonText: 'Reload Dashboard'
                }).then(() => {
                    loadUserTools();
                });

            }

        });

    }, 10000);
}

/* ==============================
   ICONS (FULL SVG – DO NOT CUT)
============================== */

function getToolIcon(toolName) {

    var calendarIcon = `...`;
    var trashIcon = `...`;
    var boltIcon = `...`;
    var gearIcon = `...`;

    var icons = {
        'Attendance Tool': calendarIcon,
        'Attandance Tool': calendarIcon,
        'Attendance Sync': calendarIcon,
        'Branch Issue': gearIcon,
        'Concurrent Simulation': boltIcon,
        'NUnit Testing Tool': gearIcon,
        'Salary Garbage': trashIcon
    };

    return icons[toolName] || gearIcon;
}

function showUnderDevelopmentModal(toolName) {
    $('#underDevToolName').text(toolName);
    new bootstrap.Modal(
        document.getElementById('underDevelopmentModal')
    ).show();
}

function escapeHtml(text) {
    if (!text) return '';
    var div = document.createElement('div');
    div.appendChild(document.createTextNode(text));
    return div.innerHTML;
}
