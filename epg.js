
/// <reference path="../../lib/jquery/dist/jquery.min.js" />
/// <reference path="../../lib/bootstrap/dist/js/bootstrap.bundle.min.js" />

//// ── Toast notification (replaces alert) ──────────────────────────────
//function showToast(message, type) {
//    type = type || 'success';
//    var cls = type === 'error' ? 'toast-error' : type === 'info' ? 'toast-info' : 'toast-success';
//    var icon = type === 'error' ? 'fa-exclamation-circle' : type === 'info' ? 'fa-info-circle' : 'fa-check-circle';
//    var $t = $('<div class="toast-msg ' + cls + '"><i class="fas ' + icon + '"></i> ' + message + '</div>');
//    $('body').append($t);
//    setTimeout(function () {
//        $t.css({ transition: 'opacity 0.3s', opacity: 0 });
//        setTimeout(function () { $t.remove(); }, 300);
//    }, 3000);
//}

//// ── Load all devices ────────────────────────────────────────────────
//function loadDevices() {
//    $.ajax({
//        url: '/Grouping/GetAllDevices',
//        type: 'GET',
//        success: function (data) {
//            var tbody = $('#tbl_grouping_body');
//            tbody.empty();
//            if (!data || data.length === 0) {
//                tbody.append('<tr><td colspan="5" class="td-empty">No devices found</td></tr>');
//                return;
//            }
//            $.each(data, function (i, d) {
//                var row =
//                    '<tr>' +
//                    '<td class="td-center"><input type="checkbox" class="chk-device row-chk" data-id="' + d.id + '" /></td>' +
//                    '<td class="td-name">' + (d.systemName || '-') + '</td>' +
//                    '<td class="td-user">' + (d.loginUser || '-') + '</td>' +
//                    '<td class="font-mono td-ip">' + (d.publicIP || '-') + '</td>' +
//                    '<td><span class="text-dash">&mdash;</span></td>' +
//                    '</tr>';
//                tbody.append(row);
//            });
//        },
//        error: function () {
//            $('#tbl_grouping_body').html('<tr><td colspan="5" class="td-error">Failed to load devices</td></tr>');
//        }
//    });
//}

//// ── Load all groups ─────────────────────────────────────────────────
//function loadGroups() {
//    $.ajax({
//        url: '/Grouping/GetAllGroups',
//        type: 'GET',
//        success: function (data) {
//            var tbody = $('#tbl_grouping_list_body');
//            tbody.empty();
//            if (!data || data.length === 0) {
//                tbody.append('<tr><td colspan="3" class="td-empty">No groups found</td></tr>');
//                return;
//            }
//            $.each(data, function (i, d) {
//                var row =
//                    '<tr>' +
//                    '<td class="td-center"><input type="checkbox" class="chk-device grp-chk" data-id="' + d.id + '" /></td>' +
//                    '<td class="td-name"><a href="javascript:void(0);" class="group-link" data-id="' + d.id + '" onclick="getDevicebygroupId(\'' + d.id + '\',\'' + d.groupName + '\');">' + (d.groupName || '-') + '</a></td>' +
//                    '<td class="td-user">' + (d.totalDeviceCount || '-') + '</td>' +
//                    '</tr>';
//                tbody.append(row);
//            });
//        },
//        error: function () {
//            $('#tbl_grouping_list_body').html('<tr><td colspan="3" class="td-error">Failed to load groups</td></tr>');
//        }
//    });
//}

//// ── Show / hide the action bar ──────────────────────────────────────
//function updateGroupButton() {
//    if ($('#panel-all').hasClass('is-hidden')) return;

//    var selectedCount = $('#panel-all .chk-device:checked').length;
//    var $bar = $('#btn-grouping-show');

//    if (selectedCount > 0) {
//        $bar.removeClass('is-hidden');
//        $('#selected-count').text(selectedCount + ' selected');
//    } else {
//        $bar.addClass('is-hidden');
//        $('#txt-group-name').val('');
//        $('#txt-group-error').removeClass('visible');
//        $('#txt-group-name').css('box-shadow', '');
//    }
//}

//// ── Assign selected devices to a group ──────────────────────────────
//function sendSelectedDevices() {
//    var groupName = $('#txt-group-name').val().trim();

//    if (!groupName) {
//        $('#txt-group-error').addClass('visible');
//        $('#txt-group-name').focus().css('box-shadow', '0 0 0 2px rgba(239, 68, 68, 0.4)');
//        return;
//    }

//    $('#txt-group-error').removeClass('visible');
//    $('#txt-group-name').css('box-shadow', '');

//    var selectedIds = [];
//    $('#panel-all .chk-device:checked').each(function () {
//        selectedIds.push($(this).data('id'));
//    });

//    if (selectedIds.length === 0) {
//        showToast('Please select at least one device.', 'info');
//        return;
//    }

//    $.ajax({
//        url: '/Grouping/AssignGroup',
//        type: 'POST',
//        contentType: 'application/json',
//        data: JSON.stringify({ deviceIds: selectedIds, groupName: groupName }),
//        headers: {
//            RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
//        },
//        success: function () {
//            showToast('Devices assigned to "' + groupName + '" successfully!');
//            $('#chk-select-all').prop('checked', false).prop('indeterminate', false);
//            $('#panel-all .chk-device').prop('checked', false);
//            $('#panel-all .chk-device').closest('tr').removeClass('row-selected');
//            updateGroupButton();
//            loadDevices();
//        },
//        error: function () {
//            showToast('An error occurred while assigning devices.', 'error');
//        }
//    });
//}

//// ── Open group detail modal ─────────────────────────────────────────
//function getDevicebygroupId(groupId, name) {
//    var modalEl = document.getElementById('groupingModel');
//    var modal = bootstrap.Modal.getInstance(modalEl);
//    if (!modal) modal = new bootstrap.Modal(modalEl);

//    document.getElementById('groupingModelLabel').textContent = name;
//    modalEl.setAttribute('data-group-id', groupId);

//    var bodyHtml =
//        '<div class="modal-search-wrap">' +
//        '<div class="modal-search-inner">' +
//        '<div class="modal-search-row">' +
//        '<input type="text" id="txt-search-device" class="modal-search-input" placeholder="Add devices...">' +
//        '<button class="btn-modal-add" id="btn-add-searched-devices" disabled onclick="addSearchedDevices()">' +
//        '<i class="fas fa-plus" style="font-size: 0.7rem;"></i> Add</button>' +
//        '</div>' +
//        '<div id="search-results-container" class="search-dropdown"></div>' +
//        '</div>' +
//        '</div>' +
//        '<div class="modal-table-wrap">' +
//        '<table class="table tbl-custom table-hover mb-0">' +
//        '<thead><tr>' +
//        '<th>Device Name</th><th>Login User</th><th>Public IP</th><th>Group</th><th>Action</th>' +
//        '</tr></thead>' +
//        '<tbody id="tbl_modal_grouping_body">' +
//        '<tr><td colspan="5" class="td-empty"><span class="load-spinner"></span> Loading...</td></tr>' +
//        '</tbody></table></div>';

//    $('#modal-grouping-body').html(bodyHtml);
//    modal.show();

//    $.ajax({
//        url: '/Grouping/GetDeviceByGroupId',
//        type: 'GET',
//        data: { groupId: groupId },
//        success: function (data) {
//            var tbody = $('#tbl_modal_grouping_body');
//            tbody.empty();
//            if (!data || data.length === 0) {
//                tbody.append('<tr><td colspan="5" class="td-empty">No devices in this group</td></tr>');
//                return;
//            }
//            $.each(data, function (i, d) {
//                var row =
//                    '<tr>' +
//                    '<td class="td-name">' + (d.systemName || '-') + '</td>' +
//                    '<td class="td-user">' + (d.loginUser || '-') + '</td>' +
//                    '<td class="font-mono td-ip">' + (d.publicIP || '-') + '</td>' +
//                    '<td><span class="badge-group">' + (d.groupName || '-') + '</span></td>' +
//                    '<td><button class="btn-trash" type="button" data-device-id="' + d.id + '" onclick="showDeleteConfirm(this)" title="Remove from group"><i class="bi bi-trash"></i></button></td>' +
//                    '</tr>';
//                tbody.append(row);
//            });
//        },
//        error: function () {
//            $('#tbl_modal_grouping_body').html('<tr><td colspan="5" class="td-error">Failed to load devices</td></tr>');
//        }
//    });
//}

//// ── Enable/disable Add button in search ─────────────────────────────
//function updateAddButton() {
//    var anyChecked = $('.chk-search-result:checked').length > 0;
//    $('#btn-add-searched-devices').prop('disabled', !anyChecked);
//}

//// ── Add searched devices to current group ───────────────────────────
//function addSearchedDevices() {
//    var groupId = document.getElementById('groupingModel').getAttribute('data-group-id');
//    var selectedIds = [];
//    $('.chk-search-result:checked').each(function () {
//        selectedIds.push($(this).data('id'));
//    });

//    if (selectedIds.length === 0) return;

//    $.ajax({
//        url: '/Grouping/UpdateAssignGroup',
//        type: 'POST',
//        contentType: 'application/json',
//        data: JSON.stringify({ deviceIds: selectedIds, groupId: groupId }),
//        headers: {
//            RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
//        },
//        success: function () {
//            var name = document.getElementById('groupingModelLabel').textContent;
//            $('#txt-search-device').val('');
//            $('#search-results-container').removeClass('open').empty();
//            loadGroups();
//            getDevicebygroupId(groupId, name);
//        },
//        error: function () {
//            showToast('Failed to add devices to group.', 'error');
//        }
//    });
//}

//// ── Delete device from group ────────────────────────────────────────
//function deleteDevicesfromEDP(deviceId) {
//    var groupId = document.getElementById('groupingModel').getAttribute('data-group-id');
//    $.ajax({
//        url: '/Grouping/DeleteDeviceFromAssignGroup',
//        type: 'POST',
//        data: { groupId: groupId, deviceId: deviceId },
//        headers: {
//            RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
//        },
//        success: function () {
//            var name = document.getElementById('groupingModelLabel').textContent;
//            loadGroups();
//            getDevicebygroupId(groupId, name);
//        },
//        error: function () {
//            showToast('Failed to remove device from group.', 'error');
//        }
//    });
//}

//// ── Delete confirmation modal logic ─────────────────────────────────
//var pendingDeleteId = null;

//function showDeleteConfirm(btn) {
//    pendingDeleteId = $(btn).data('device-id');
//    var modal = new bootstrap.Modal(document.getElementById('deleteConfirmModal'));
//    modal.show();
//}

//document.getElementById('btn-confirm-delete').addEventListener('click', function () {
//    if (pendingDeleteId) {
//        deleteDevicesfromEDP(pendingDeleteId);
//        pendingDeleteId = null;
//        var modal = bootstrap.Modal.getInstance(document.getElementById('deleteConfirmModal'));
//        modal.hide();
//    }
//});

//document.getElementById('deleteConfirmModal').addEventListener('hidden.bs.modal', function () {
//    pendingDeleteId = null;
//});

//// ── Document ready ──────────────────────────────────────────────────
//$(document).ready(function () {
//    var params = new URLSearchParams(window.location.search);
//    var edp = params.get('epg') || 'all';
//    var selected = params.get('selected') || 'all';

//    // Start both panels hidden
//    $('#panel-all').addClass('is-hidden');
//    $('#panel-group').addClass('is-hidden');

//    // Activate correct tab + show correct panel
//    if (edp === 'group' && selected === 'group') {
//        $('.tab-btn').removeClass('tab-active');
//        $('.tab-btn[data-tab="group"]').addClass('tab-active');
//        $('#panel-group').removeClass('is-hidden');
//        loadGroups();
//    } else {
//        $('.tab-btn').removeClass('tab-active');
//        $('.tab-btn[data-tab="all"]').addClass('tab-active');
//        $('#panel-all').removeClass('is-hidden');
//        loadDevices();
//    }

//    // ── Parent checkbox → toggle all row checkboxes ────────────────
//    $(document).on('change', '#chk-select-all', function () {
//        var isChecked = $(this).is(':checked');
//        $('#panel-all .chk-device').prop('checked', isChecked);
//        $('#panel-all .chk-device').closest('tr').toggleClass('row-selected', isChecked);
//        updateGroupButton();
//    });

//    // ── Individual checkbox → update states ────────────────────────
//    $(document).on('change', '#panel-all .chk-device', function () {
//        $(this).closest('tr').toggleClass('row-selected', $(this).is(':checked'));

//        var total = $('#panel-all .chk-device').length;
//        var checked = $('#panel-all .chk-device:checked').length;
//        $('#chk-select-all').prop('indeterminate', checked > 0 && checked < total);
//        $('#chk-select-all').prop('checked', checked === total);
//        updateGroupButton();
//    });

//    // ── Clear selection button ─────────────────────────────────────
//    $(document).on('click', '#btn-clear-selection', function () {
//        $('#panel-all .chk-device').prop('checked', false);
//        $('#panel-all .chk-device').closest('tr').removeClass('row-selected');
//        $('#chk-select-all').prop('checked', false).prop('indeterminate', false);
//        updateGroupButton();
//    });

//    // ── Assign group button ────────────────────────────────────────
//    $(document).on('click', '#btn-add-to-group', function () {
//        sendSelectedDevices();
//    });

//    // ── Clear validation on typing ─────────────────────────────────
//    $(document).on('input', '#txt-group-name', function () {
//        if ($(this).val().trim()) {
//            $('#txt-group-error').removeClass('visible');
//            $(this).css('box-shadow', '');
//        }
//    });

//    // ── Enter key to submit ────────────────────────────────────────
//    $(document).on('keydown', '#txt-group-name', function (e) {
//        if (e.key === 'Enter') { e.preventDefault(); sendSelectedDevices(); }
//    });

//    // ── Tab switching ──────────────────────────────────────────────
//    $(document).on('click', '.tab-btn', function (e) {
//        e.preventDefault();

//        var $clicked = $(this);
//        var tab = $clicked.data('tab');

//        // Update URL
//        var newUrl = tab === 'all' ? '?epg=all&selected=all' : '?epg=group&selected=group';
//        window.history.pushState({}, '', newUrl);

//        // Toggle active tab
//        $('.tab-btn').removeClass('tab-active');
//        $clicked.addClass('tab-active');

//        // Hide panels + action bar
//        $('#panel-all').addClass('is-hidden');
//        $('#panel-group').addClass('is-hidden');
//        $('#btn-grouping-show').addClass('is-hidden');

//        // Show correct panel
//        if (tab === 'all') {
//            $('#panel-all').removeClass('is-hidden');
//            loadDevices();
//        } else {
//            $('#panel-group').removeClass('is-hidden');
//            loadGroups();
//        }
//    });

//    // ── Modal: parent checkbox ─────────────────────────────────────
//    $(document).on('change', '#chk-modal-select-all', function () {
//        $('.chk-modal-device').prop('checked', $(this).is(':checked'));
//    });

//    // ── Search: live search unmapped devices ───────────────────────
//    var searchTimeout = null;
//    $(document).on('input', '#txt-search-device', function () {
//        var keyword = $(this).val().trim();
//        var $container = $('#search-results-container');

//        clearTimeout(searchTimeout);

//        if (keyword.length === 0) {
//            $container.removeClass('open').empty();
//            $('#btn-add-searched-devices').prop('disabled', true);
//            return;
//        }

//        searchTimeout = setTimeout(function () {
//            $.ajax({
//                url: '/Grouping/GetUnmappedDevices',
//                type: 'GET',
//                data: { keyword: keyword },
//                success: function (data) {
//                    $container.empty();

//                    if (!data || data.length === 0) {
//                        $container.html('<div class="search-empty">No matching devices found</div>').addClass('open');
//                        $('#btn-add-searched-devices').prop('disabled', true);
//                        return;
//                    }

//                    $.each(data, function (i, d) {
//                        var item =
//                            '<div class="search-item">' +
//                            '<input type="checkbox" class="chk-search-result" data-id="' + d.id + '" data-name="' + (d.systemName || '') + '" />' +
//                            '<span class="search-item-name">' + (d.systemName || '-') + '</span>' +
//                            '<span class="search-item-ip">' + (d.publicIP || '') + '</span>' +
//                            '</div>';
//                        $container.append(item);
//                    });

//                    $container.addClass('open');
//                },
//                error: function () {
//                    $container.html('<div class="search-fail">Search failed</div>').addClass('open');
//                }
//            });
//        }, 300);
//    });

//    // ── Search: clicking row toggles checkbox ──────────────────────
//    $(document).on('click', '.search-item', function (e) {
//        if ($(e.target).is('input[type=checkbox]')) return;
//        var $chk = $(this).find('.chk-search-result');
//        $chk.prop('checked', !$chk.prop('checked'));
//        updateAddButton();
//    });

//    $(document).on('change', '.chk-search-result', function () {
//        updateAddButton();
//    });

//    // ── Close search dropdown on outside click ─────────────────────
//    $(document).on('click', function (e) {
//        if (!$(e.target).closest('#txt-search-device, .search-dropdown, #btn-add-searched-devices').length) {
//            $('#search-results-container').removeClass('open');
//        }
//    });
//});


function loadDevices() {
    $.ajax({
        url: '/Grouping/GetAllDevices',
        type: 'GET',
        beforeSend: function () {
            loadingStart();
        },
        success: function (data) {
            var tbody = $('#tbl_grouping_body');
            tbody.empty();
            if (!data || data.length === 0) {
                tbody.append('<tr><td colspan="5" style="text-align:center;">no devices found...</td></tr>');
                return;
            }
            $.each(data, function (i, device) {
                var row = '<tr>' +
                    '<td><input type="checkbox" class="chk-device" data-id="' + device.id + '" /></td>' +
                    '<td>' + (device.systemName || '-') + '</td>' +
                    '<td>' + (device.loginUser || '-') + '</td>' +
                    '<td>' + (device.publicIP || '-') + '</td>' +
                    // '<td>' + statusBadge + '</td>' +
                    '<td></td>' +   // Group column — left empty
                    '</tr>';
                tbody.append(row);
            });
        },
        error: function () {
            $('#tbl_grouping_body').html(
                '<tr><td colspan="5" style="text-align:center;color:red;">failed to load devices...</td></tr>'
            );
        },
        complete: function () {
            loadingStop(); // Always called after success or error 
        }
    });
}

function loadGroups() {
    $.ajax({
        url: '/Grouping/GetAllGroups',
        type: 'GET',
        beforeSend: function () {
            loadingStart();
        },
        success: function (data) {
            console.log(data);
            var tbody = $('#tbl_grouping_list_body');
            tbody.empty();
            if (!data || data.length === 0) {
                tbody.append('<tr><td colspan="5" style="text-align:center;">no devices found...</td></tr>');
                return;
            }
            $.each(data, function (i, device) {
                // ✅ Fixed: wrap HTML in quotes + escape potential quotes in groupName
                let isEnabled = device.isGroupPatchEnabled;
                let statusBadge = `
                                    <div class="group-toggle-switch ${isEnabled ? 'toggle-on' : 'toggle-off'}"
                                        onclick="${isEnabled
                    ? `toggleGroupDirect('${device.id}', '${device.groupName}','${device.groupPatchScheduledTimeId}')`
                    : `openScheduleModal('${device.id}', '${device.groupName}','${device.groupPatchScheduledTimeId}')`
                                                    }">
                                        <span class="toggle-label">${isEnabled ? 'Enabled' : 'Disabled'}</span>
                                        <span class="toggle-knob"></span>
                                    </div>`;
                let $scheduledTime = "-";
                if (device.scheduledTime !== null) {
                    $scheduledTime = "";
                    let localDate = new Date(device.scheduledTime);
                    $scheduledTime = localDate.toLocaleString();
                }

                let row = '<tr>' +
                    '<td></td>' +
                    '<td><a href="javascript:void(0);" data-id="' + device.id + '" onclick="getDevicebygroupId(\'' + device.id + '\',\'' + device.groupName + '\');">' + (device.groupName || '-') + '</a></td>' +
                    '<td>' + (device.totalDeviceCount || '-') + '</td>' +
                    '<td>' + statusBadge +'</td>' +
                    '<td>' + $scheduledTime + '</td>' +
                    '</tr>';
                tbody.append(row);
            });
        },
        error: function () {
            $('#tbl_grouping_list_body').html(
                '<tr><td colspan="5" style="text-align:center;color:red;">Failed to load devices.</td></tr>'
            );
        },
        complete: function () {
            loadingStop(); // Always called after success or error 
        }
    });
}

function openScheduleModal(groupId, groupName, gpshistoryId) {
    // Set modal title
    $('#groupingModelLabel').text('Schedule Time for Group: ' + groupName);

    // Inject body
    $('#modal-grouping-body').html(`
        <input type="hidden" id="schedule_group_id" value="${groupId}" />

        <div class="mb-3">
            <label for="schedule_status_select" class="form-label">Status</label>
            <select class="form-select" id="schedule_status_select" onchange="onScheduleStatusChange()" disabled>
                <option value="" disabled>-- Select --</option>
                <option value="enabled" selected>Enabled</option>
                <option value="disabled">Disabled</option>
            </select>
        </div>

        <div class="mb-3">
            <label for="schedule_datetime" class="form-label">Scheduled Date & Time</label>
            <input type="datetime-local" class="form-control" id="schedule_datetime"
                   oninput="onScheduleDateTimeChange()" />
            <div class="form-text text-danger d-none" id="schedule_dt_error">
                Please select current date/time or a future date/time.
            </div>
        </div>
        <input id="gpshistoryId" type="hidden" value="${gpshistoryId}"/>
    `);

    // Inject footer
    $('#groupingModel .modal-footer').html(`
        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
        <button type="button" class="btn btn-primary" id="schedule_save_btn" disabled
                onclick="saveSchedule()">Save</button>
    `);

    new bootstrap.Modal(document.getElementById('groupingModel')).show();
}

function onScheduleStatusChange() {
    var status = document.getElementById('schedule_status_select').value;
    var dtInput = document.getElementById('schedule_datetime');
    var saveBtn = document.getElementById('schedule_save_btn');
    var dtError = document.getElementById('schedule_dt_error');

    if (status === 'enabled') {
        dtInput.disabled = false;
        onScheduleDateTimeChange(); // re-validate if value already exists
    } else if (status === 'disabled') {
        dtInput.disabled = true;
        dtInput.value = '';
        dtError.classList.add('d-none');
        saveBtn.disabled = false; // no datetime needed when disabling
    } else {
        dtInput.disabled = true;
        saveBtn.disabled = true;
    }
}

function onScheduleDateTimeChange() {
    var status = document.getElementById('schedule_status_select').value;
    if (status !== 'enabled') return;

    var dtInput = document.getElementById('schedule_datetime');
    var saveBtn = document.getElementById('schedule_save_btn');
    var dtError = document.getElementById('schedule_dt_error');

    if (!dtInput.value) {
        saveBtn.disabled = true;
        dtError.classList.add('d-none');
        return;
    }

    var selectedDate = new Date(dtInput.value);
    var now = new Date();
    now.setSeconds(0, 0);

    if (selectedDate >= now) {
        saveBtn.disabled = false;
        dtError.classList.add('d-none');
    } else {
        saveBtn.disabled = true;
        dtError.classList.remove('d-none');
    }
}
//Group Patch Scheduled Time HistoryId : gpshistoryId
function toggleGroupDirect(groupId, groupName, gpshistoryId) {
    if (!confirm(`Are you sure you want to disable "${groupName}" group patching?`)) return;

    $.ajax({
        url: '/Grouping/SaveSchedule',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            groupId: groupId,
            isEnabled: false,       // toggling from enabled → disabled
            scheduledTime: null,
            gpshistoryId: gpshistoryId
        }),
        beforeSend: function () { loadingStart(); },
        success: function () {
            loadGroups(); // Refresh table
        },
        error: function () {
            alert('Failed to update group. Please try again.');
        },
        complete: function () { loadingStop(); }
    });
}

function saveSchedule() {
    var groupId = document.getElementById('schedule_group_id').value;
    var status = document.getElementById('schedule_status_select').value;
    var scheduledTime = document.getElementById('schedule_datetime').value || null;
    let gpshistoryId = document.getElementById('gpshistoryId').value;

    $.ajax({
        url: '/Grouping/SaveSchedule',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            groupId: groupId,
            isEnabled: status === 'enabled',
            scheduledTime: scheduledTime,
            gpshistoryId: gpshistoryId
        }),
        beforeSend: function () { loadingStart(); },
        success: function () {
            bootstrap.Modal.getInstance(document.getElementById('groupingModel')).hide();
            loadGroups();
        },
        error: function () {
            alert('Failed to save schedule. Please try again.');
        },
        complete: function () { loadingStop(); }
    });
}

// ── Show / hide the dynamic "Add to Group" button + Group Name input ──
function updateGroupButton() {
    let selectedCount = $('.chk-device:checked').length;
    const bar = document.getElementById('btn-grouping-show');
    const count = document.getElementById('selected-count');
    $('#txt-group-name').val(''); // clear existing value

    if (selectedCount > 0) {
        bar.classList.remove('hidden');
        count.textContent = `${selectedCount} selected`;
    } else {
        bar.classList.add('hidden');
    }
}

//function updateGroupButton() {
//    var selectedCount = $('.chk-device:checked').length;
//    var $container = $('#btn-grouping-show');

//    if (selectedCount > 0) {
//        if ($('#btn-add-to-group').length === 0) {
//            // Create textbox + button together
//            var html =
//                '<div style="display:flex; align-items:flex-start; gap:8px; flex-wrap:wrap;">' +
//                '<div style="display:flex; flex-direction:column;">' +
//                '<input type="text" id="txt-group-name" class="form-control form-control-sm" ' +
//                'placeholder="enter group name..." ' +
//                'style="width:220px;" />' +
//                '<span id="txt-group-error" style="color:red; font-size:12px; display:none;">please enter group name</span>' +
//                '</div>' +
//                '<button id="btn-add-to-group" class="btn btn-success btn-sm" onclick="sendSelectedDevices()">' +
//                '<i class="fas fa-layer-group"></i> Create Group ' +
//                '<span id="selected-count" class="badge bg-light text-dark ms-1"></span>' +
//                '</button>' +
//                '</div>';
//            $container.html(html);
//        }
//        $('#selected-count').text(selectedCount + ' selected');
//    } else {
//        $container.empty();
//    }
//}

// ── Collect checked IDs, validate group name, then POST to server ──────
function sendSelectedDevices() {
    var groupName = $('#txt-group-name').val().trim();

    // Validation
    if (!groupName) {
        $('#txt-group-error').show();
        $('#txt-group-name').css('border-color', 'red').focus();
        return;
    }

    // Clear validation state
    $('#txt-group-error').hide();
    $('#txt-group-name').css('border-color', '');

    var selectedIds = [];
    $('.chk-device:checked').each(function () {
        selectedIds.push($(this).data('id'));
    });

    if (selectedIds.length === 0) {
        alert('Please select at least one device.');
        return;
    }

    $.ajax({
        url: '/Grouping/AssignGroup',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ deviceIds: selectedIds, groupName: groupName }),
        headers: {
            RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        beforeSend: function () {
            loadingStart();
        },
        success: function (response) {
            alert('Devices assigned to group "' + groupName + '" successfully!');
            $('#chk-select-all').prop('checked', false).prop('indeterminate', false);
            document.getElementById('btn-grouping-show').classList.add('hidden');
            loadDevices();
        },
        error: function () {
            alert('An error occurred while assigning devices.');
        },
        complete: function () {
            loadingStop(); // Always called after success or error 
        }
    });
}

function getDevicebygroupId(groupId, name) {
    const modalEl = document.getElementById('groupingModel');
    let modal = bootstrap.Modal.getInstance(modalEl);
    if (!modal) modal = new bootstrap.Modal(modalEl);

    document.getElementById('groupingModelLabel').innerHTML = '<h4>' + name + '</h4>';
    modalEl.setAttribute('data-group-id', groupId);

    // ── Build modal body dynamically ──────────────────────────────────
    var bodyHtml =
        // Search bar row
        '<div style="display:flex; justify-content:flex-end; align-items:flex-start; margin-bottom:10px; gap:0;">' +
        '<div style="display:flex; flex-direction:column;">' +
        '<div class="input-group input-group-sm">' +
        '<input type="text" id="txt-search-device" class="form-control" placeholder="add devices..." style="width:220px;" />' +
        '<button class="btn btn-success" id="btn-add-searched-devices" disabled  onclick="addSearchedDevices()">' +
        '<i class="fas fa-plus"></i> Add' +
        '</button>' +
        '</div>' +
        '<div id="search-results-container" style="position:absolute; z-index:9999; background:white; border:1px solid #dee2e6; border-radius:4px; width:220px; max-height:200px; overflow-y:auto; display:none; margin-top:30px;">' +        
        '</div>' +
        '</div>' +
        '</div>' +

        // Table wrapper
        '<div style="min-height:100px; max-height:calc(100vh - 350px); overflow:auto;">' +
        '<table id="tbl_modal_grouping" class="table" style="text-align:center;">' +
        '<thead>' +
        '<tr>' +
        //'<th scope="col"><input type="checkbox" id="chk-modal-select-all" /></th>' +
        '<th scope="col">Device Name</th>' +
        '<th scope="col">Login User</th>' +
        '<th scope="col">Public IP</th>' +
        '<th scope="col">Group</th>' +
        '<th scrope="col">Action</th>'+
        '</tr>' +
        '</thead>' +
        '<tbody id="tbl_modal_grouping_body">' +
        // Loading spinner while AJAX runs
        '<tr><td colspan="5" style="text-align:center;">' +
        '<div class="spinner-border spinner-border-sm text-primary" role="status"></div>' +
        ' Loading...' +
        '</td></tr>' +
        '</tbody>' +
        '</table>' +
        '</div>';

    $('#modal-grouping-body').html(bodyHtml);

    // Show modal immediately — table populates after AJAX
    modal.show();

    // ── Fetch devices for this group ──────────────────────────────────
    $.ajax({
        url: '/Grouping/GetDeviceByGroupId',
        type: 'GET',
        data: { groupId: groupId },
        beforeSend: function () {
            loadingStart();
        },
        success: function (data) {
            console.log('model');
            console.log(data);
            var tbody = $('#tbl_modal_grouping_body');
            tbody.empty();
            if (!data || data.length === 0) {
                tbody.append('<tr><td colspan="5" style="text-align:center;">No devices found.</td></tr>');
                return;
            }
            $.each(data, function (i, device) {
                var row =
                    '<tr>' +
                    //'<td><input type="checkbox" class="chk-modal-device" data-id="' + (device.id || '') + '" /></td>' +
                    '<td>' + (device.systemName || '-') + '</td>' +
                    '<td>' + (device.loginUser || '-') + '</td>' +
                    '<td>' + (device.publicIP || '-') + '</td>' +
                    '<td>' + (device.groupName || '-') + '</td>' +
                    '<td><button class="btn-trash-devices-grouping" type="button" data-bs-toggle="tooltip" data-bs-placement="left" title="Delete" data-device-id="' + device.id + '" onclick="showDeleteConfirm(this)"><i class="bi bi-trash"></i></button></td>';
                    '</tr>';
                tbody.append(row);
            });
        },
        error: function () {
            $('#tbl_modal_grouping_body').html(
                '<tr><td colspan="5" style="text-align:center; color:red;">Failed to load devices.</td></tr>'
            );
        },
        complete: function () {
            loadingStop(); // Always called after success or error 
        }
    });
}

function updateAddButton() {
    var anyChecked = $('.chk-search-result:checked').length > 0;
    $('#btn-add-searched-devices').prop('disabled', !anyChecked);
}

// ── Add: send selected searched devices to the current group ──────────
function addSearchedDevices() {
    var groupId = document.getElementById('groupingModel').getAttribute('data-group-id');
    var selectedIds = [];
    $('.chk-search-result:checked').each(function () {
        selectedIds.push($(this).data('id'));
    });

    if (selectedIds.length === 0) return;

    $.ajax({
        url: '/Grouping/UpdateAssignGroup',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ deviceIds: selectedIds, groupId: groupId }),
        headers: {
            RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        beforeSend: function () {
            loadingStart();
        },
        success: function () {
            // Refresh modal table with updated group devices
            var name = document.getElementById('groupingModelLabel').innerText;
            $('#txt-search-device').val('');
            $('#search-results-container').hide().empty();
            $('#btn-add-searched-devices').hide();
            loadGroups();
            getDevicebygroupId(groupId, name);

        },
        error: function () {
            alert('Failed to add devices to group.');
        },
        complete: function () {
            loadingStop(); // Always called after success or error 
        }
    });
}

//delete devices from endpoint group (EDP)
function deleteDevicesfromEDP(deviceId) {    
    var groupId = document.getElementById('groupingModel').getAttribute('data-group-id');
    $.ajax({
        url: '/Grouping/DeleteDeviceFromAssignGroup',
        type: 'POST',
        data: { groupId: groupId, deviceId: deviceId },
        headers: {
            RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        beforeSend: function () {
            loadingStart();
        },
        success: function () {
            // Refresh modal table with updated group devices
            var name = document.getElementById('groupingModelLabel').innerText;
            $('#txt-search-device').val('');
            $('#search-results-container').hide().empty();
            $('#btn-add-searched-devices').hide();
            loadGroups();
            getDevicebygroupId(groupId, name);

        },
        error: function () {
            alert('Failed to add devices to group.');
        },
        complete: function () {
            loadingStop(); // Always called after success or error 
        }
    });
}
let pendingDeleteId = null;
function showDeleteConfirm(btn) {
    pendingDeleteId = $(btn).data('device-id');
    const modal = new bootstrap.Modal(document.getElementById('deleteConfirmModal'));
    modal.show();
}
// Handle the "Delete" button click inside the modal
document.getElementById('btn-confirm-delete').addEventListener('click', function () {
    if (pendingDeleteId) {
        deleteDevicesfromEDP(pendingDeleteId);
        pendingDeleteId = null;

        // Close modal
        const modalEl = document.getElementById('deleteConfirmModal');
        const modal = bootstrap.Modal.getInstance(modalEl);
        modal.hide();
    }
});
// Reset pending ID when modal is hidden (in case user cancels)
document.getElementById('deleteConfirmModal').addEventListener('hidden.bs.modal', function () {
    pendingDeleteId = null;
});


$(document).ready(function () {
    const params = new URLSearchParams(window.location.search);
    const edp = params.get('epg') || 'all'; // default = all
    const selected = params.get('selected') || 'all';
    const $allTabs = $('#epg-btn-group a');
    const $tables = $('#div-tbl-container table');

    if (edp !== selected) {
        // Reset all buttons
        $allTabs.removeClass('active btn-success')
            .addClass('btn-outline-success')
            .removeAttr('aria-current');
    }
    // Collapse all tables
    $tables.addClass('table-collapse');

    // Activate correct tab + load data
    if (edp === 'all') {
        $('#epg-btn-group a[data-tab="all"]')
            .removeClass('btn-outline-success')
            .addClass('btn-success active')
            .attr('aria-current', 'page');


        $('#tbl_grouping').removeClass('table-collapse');
        loadDevices();

    } else if (edp === 'group') {
        $('#epg-btn-group a[data-tab="group"]')
            .removeClass('btn-outline-success')
            .addClass('btn-success active')
            .attr('aria-current', 'page');

        $('#tbl_grouping_list').removeClass('table-collapse');
        loadGroups();
    } else {
        $('#epg-btn-group a[data-tab="all"]')
            .removeClass('btn-outline-success')
            .addClass('btn-success active')
            .attr('aria-current', 'page');


        $('#tbl_grouping').removeClass('table-collapse');
        loadDevices();
    }

    // ── 1. Load devices on page load ──────────────────────────────
    //loadDevices();

    // ── 2. Parent checkbox → toggle all row checkboxes ────────────
    $(document).on('change', '#chk-select-all', function () {
        $('.chk-device').prop('checked', $(this).is(':checked'));
        updateGroupButton();
    });

    // ── 3. Individual checkbox → update button visibility ─────────
    $(document).on('change', '.chk-device', function () {
        var total = $('.chk-device').length;
        var checked = $('.chk-device:checked').length;
        $('#chk-select-all').prop('indeterminate', checked > 0 && checked < total);
        $('#chk-select-all').prop('checked', checked === total);
        updateGroupButton();
    });

    $(document).on('click', '#epg-btn-group a', function (e) {
        e.preventDefault(); // Prevent default anchor jump        
        $('#chk-select-all').prop('checked', false).prop('indeterminate', false);

        const $clicked = $(this);
        const $allTabs = $('#epg-btn-group a');
        const $tables = $('#div-tbl-container table');
        document.getElementById('btn-grouping-show').classList.add('hidden');

        // ✅ Update URL query string
        const tab = $clicked.data('tab');
        const newUrl = tab === 'all' ? '?epg=all&selected=all' : '?epg=group&selected=group';
        window.history.pushState({}, '', newUrl);

        // 1. Reset ALL buttons to inactive state (btn-primary, no active)
        $allTabs.removeClass('active btn-success').addClass('btn-outline-success').removeAttr('aria-current');

        // 2. Set clicked button to active state (btn-success + active)
        $clicked.removeClass('btn-outline-success').addClass('btn-success active').attr('aria-current', 'page');

        // 3. Collapse all tables first (clean state)
        $tables.addClass('table-collapse');

        // 4. Show the relevant table based on tab
        let $tab = $clicked.data('tab');
        if ($tab === 'all') {
            $('#tbl_grouping').removeClass('table-collapse');
            loadDevices();
        } else {
            $('#tbl_grouping_list').removeClass('table-collapse');
            loadGroups();
        }
    });


    // ── Modal: parent checkbox selects all modal rows ─────────────────────
    $(document).on('change', '#chk-modal-select-all', function () {
        $('.chk-modal-device').prop('checked', $(this).is(':checked'));
    });

    // ── Search: live search unmapped devices ──────────────────────────────
    var searchTimeout = null;
    $(document).on('input', '#txt-search-device', function () {
        var keyword = $(this).val().trim();
        var $container = $('#search-results-container');

        clearTimeout(searchTimeout);

        if (keyword.length === 0) {
            $container.hide().empty();
            $('#btn-add-searched-devices').prop('disabled', true);
            return;
        }

        // Debounce — wait 300ms after user stops typing
        searchTimeout = setTimeout(function () {
            $.ajax({
                url: '/Grouping/GetUnmappedDevices',
                type: 'GET',
                data: { keyword: keyword },
                beforeSend: function () {
                    loadingStart();
                },
                success: function (data) {
                    $container.empty();

                    if (!data || data.length === 0) {
                        $container.append(
                            '<div style="padding:8px 12px; color:#6c757d; font-size:12px;">No matching devices found.</div>'
                        );
                        $('#btn-add-searched-devices').prop('disabled', true);
                        $container.show();
                        return;
                    }

                    $.each(data, function (i, device) {
                        var item =
                            '<div class="search-result-item" style="display:flex; align-items:center; gap:8px; padding:6px 12px; border-bottom:1px solid #f0f0f0; font-size:12px; cursor:pointer; min-height:50px; max-height:50px; overflow-x:scroll;">' +
                            '<input type="checkbox" class="chk-search-result" data-id="' + device.id + '" data-name="' + (device.systemName || '') + '" />' +
                            '<span>' + (device.systemName || '-') + '</span>' +
                            '<span style="color:#6c757d; margin-left:auto;">' + (device.publicIP || '') + '</span>' +
                            '</div>';
                        $container.append(item);
                    });

                    $container.show();
                },
                error: function () {
                    $container.empty()
                        .append('<div style="padding:8px 12px; color:red; font-size:12px;">Search failed.</div>')
                        .show();
                },
                complete: function () {
                    loadingStop(); // Always called after success or error 
                }
            });
        }, 300);
    });

    // ── Search: clicking row label toggles its checkbox ───────────────────
    $(document).on('click', '.search-result-item', function (e) {
        if ($(e.target).is('input[type=checkbox]')) return; // already handled
        var $chk = $(this).find('.chk-search-result');
        $chk.prop('checked', !$chk.prop('checked'));
        updateAddButton();
    });

    $(document).on('change', '.chk-search-result', function () {
        updateAddButton();
    });
    // ── Close search results when clicking outside ────────────────────────
    $(document).on('click', function (e) {
        if (!$(e.target).closest('#txt-search-device, #search-results-container').length) {
            $('#search-results-container').hide();
        }
    });

    // ── Clear validation error on typing ─────────────────────────────────
    $(document).on('input', '#txt-group-name', function () {
        if ($(this).val().trim()) {
            $('#txt-group-error').hide();
            $(this).css('border-color', '');
        }
    });

});
