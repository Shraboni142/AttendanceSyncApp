/* ==============================
   User Dashboard - Tool Cards
   (ORIGINAL UI - FIXED)
============================== */

$(function () {
    loadUserTools(); // ONLY this
});

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

        $.each(res.Data, function (_, tool) {

            var cardClass = tool.IsImplemented
                ? 'implemented'
                : 'under-development';

            // ✅ IMPORTANT FIX (ToolName)
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

            if (isImplemented && route) {
                window.location.href = route.replace('~/', APP.baseUrl);
            } else {
                showUnderDevelopmentModal(toolName);
            }
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
   ICONS (FULL SVG – DO NOT CUT)
============================== */

function getToolIcon(toolName) {

    var calendarIcon = `
<svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" fill="white" viewBox="0 0 16 16">
<path d="M3.5 0a.5.5 0 0 1 .5.5V1h8V.5a.5.5 0 0 1 1 0V1h1a2 2 0 0 1 2 2v11a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2V3a2 2 0 0 1 2-2h1V.5a.5.5 0 0 1 .5-.5z"/>
</svg>`;

    var trashIcon = `
<svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" fill="white" viewBox="0 0 16 16">
<path d="M5.5 5.5A.5.5 0 0 1 6 6v7a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5z"/>
<path d="M8 5.5a.5.5 0 0 1 .5.5v7a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5z"/>
<path d="M10.5 5.5a.5.5 0 0 1 .5.5v7a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5z"/>
<path d="M14.5 3a1 1 0 0 1-1 1H13v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V4h-.5a1 1 0 0 1 0-2h3a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1h3a1 1 0 0 1 1 1z"/>
</svg>`;

    var boltIcon = `
<svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" fill="white" viewBox="0 0 16 16">
<path d="M11.3 1L1 9h5l-1 6 10-8H9l2.3-6z"/>
</svg>`;

    var gearIcon = `
<svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" fill="white" viewBox="0 0 16 16">
<path d="M8 4.754a3.246 3.246 0 1 0 0 6.492 3.246 3.246 0 0 0 0-6.492z"/>
</svg>`;

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
    new bootstrap.Modal(document.getElementById('underDevelopmentModal')).show();
}

function escapeHtml(text) {
    if (!text) return '';
    var div = document.createElement('div');
    div.appendChild(document.createTextNode(text));
    return div.innerHTML;
}
