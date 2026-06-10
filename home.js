/// <reference path="../../lib/jquery/dist/jquery.min.js" />
/// <reference path="../../plugins/popper/popper.min.js" />

// ── Active tab tracker ────────────────────────────────────────────────────────
let _activeHomeTab = 'dashboard'; // dashboard | approved | unapproved

// ─── Tooltip helper ──────────────────────────────────────────────────────────
function initializeTooltips() {
    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => {
        const existing = bootstrap.Tooltip.getInstance(el);
        if (existing) existing.dispose();
    });
    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => {
        new bootstrap.Tooltip(el);
    });
}

// ─── On DOM Ready ────────────────────────────────────────────────────────────
$(document).ready(function () {

    // ── Page load pe saved tab restore karo ──────────────────────────────
    const savedTab = localStorage.getItem('homeActiveTab');
    if (savedTab && savedTab !== 'dashboard') {
        const $tabLink = $(`.device-model-nav .nav-link[data-type="${savedTab}"]`);
        if ($tabLink.length) {
            $tabLink.trigger('click');
        }
    }
    // ─────────────────────────────────────────────────────────────────────

    // ── Home page search (approved / unapproved tabs) ─────────────────────
    $('#home-search-input').on('input', function () {
        const searchVal = $(this).val().toLowerCase().trim();

        const tbodyId = _activeHomeTab === 'approved'
            ? '#approvedtablebody'
            : '#unapprovedtablebody';

        _clearNoResultRow();

        if (searchVal === '') {
            $(tbodyId + ' tr').removeClass('home-row-hidden');
            return;
        }

        let visibleCount = 0;
        $(tbodyId + ' tr').each(function () {
            const rowText = $(this).text().toLowerCase();
            const matches = rowText.includes(searchVal);
            $(this).toggleClass('home-row-hidden', !matches);
            if (matches) visibleCount++;
        });

        if (visibleCount === 0) {
            const colCount = $(tbodyId).closest('table').find('thead th').length;
            $(tbodyId).append(
                `<tr class="home-no-result-row">
                    <td colspan="${colCount}">No matching records found.</td>
                </tr>`
            );
        }
    });

    $('#home-search-input').on('search', function () {
        if ($(this).val() === '') {
            const tbodyId = _activeHomeTab === 'approved'
                ? '#approvedtablebody'
                : '#unapprovedtablebody';
            _clearNoResultRow();
            $(tbodyId + ' tr').removeClass('home-row-hidden');
        }
    });
    // ─────────────────────────────────────────────────────────────────────

    // ── Existing txt_patch_search — unchanged ─────────────────────────────
    $('#txt_patch_search').on('keyup', function () {
        var searchValue = $(this).val().toLowerCase().trim();
        var targetTable = $(this).data('target-table');
        if (!targetTable) return;

        $('#' + targetTable + ' tbody tr.no-data-row').remove();

        if (searchValue === '') {
            $('#' + targetTable + ' tbody tr').show();
        } else {
            var visibleCount = 0;
            $('#' + targetTable + ' tbody tr').each(function () {
                var isVisible = $(this).text().toLowerCase().indexOf(searchValue) > -1;
                $(this).toggle(isVisible);
                if (isVisible) visibleCount++;
            });
            if (visibleCount === 0) {
                var colCount = $('#' + targetTable + ' thead th').length;
                $('#' + targetTable + ' tbody').append(
                    '<tr class="no-data-row"><td colspan="' + colCount + '" style="text-align:center;padding:10px 20px;color:#6c757d;">no data found...</td></tr>'
                );
            }
        }
    });

    $('#txt_patch_search').on('search', function () {
        var targetTable = $(this).data('target-table');
        if (!targetTable) return;
        $('#' + targetTable + ' tbody tr.no-data-row').remove();
        $('#' + targetTable + ' tbody tr').show();
    });
    // ─────────────────────────────────────────────────────────────────────

});

// ─── Tab switching ────────────────────────────────────────────────────────────
$(document).on('click', '.device-model-nav .nav-link', function () {
    $(this).closest('ul').find('.nav-link').removeClass('active');
    $(this).addClass('active');

    const type = $(this).data('type');
    const tabId = $(this).data('tabid');

    _activeHomeTab = type;

    // ── Active tab localStorage mein save karo ──
    localStorage.setItem('homeActiveTab', type);

    if (tabId) {
        $('.tab-container').hide();
        $('#' + tabId).show();
    }

    // Search box — dashboard pe hide, baaki tabs pe show
    if (type === 'dashboard') {
        $('#home-search-wrapper').hide();
        $('#home-search-input').val('');
        Pagination.hide();
        return;
    }

    $('#home-search-wrapper').show();
    $('#home-search-input').val('');
    _clearNoResultRow();

    Pagination.show();

    if (type === 'approved') {
        Pagination.activate('approved');
        const s = Pagination.getState('approved');
        getapproved(s.page, s.pageSize);
        Pagination.init({
            key: 'approved',
            loader: getapproved,
            pageSize: 25,
            scrollSelector: '.div-approved-table',
            active: true
        });
    }
    if (type === 'unapproved') {
        Pagination.activate('unapproved');
        const s = Pagination.getState('unapproved');
        getunapproved(s.page, s.pageSize);
        Pagination.init({
            key: 'unapproved',
            loader: getunapproved,
            pageSize: 25,
            scrollSelector: '.div-unapproved-table'
        });
    }
    if (type === 'availablepatches' || type === 'installedpatches') reloadWithType(type);
    if (type === 'availablesoftwares' || type === 'installedsoftwares') reloadWithType(type);
});


// ─── Load approved devices (paged) ───────────────────────────────────────────
function getapproved(page, pageSize) {
    page = page || 1;
    pageSize = pageSize || 25;

    loadingStart();

    fetch(`/Workstation/GetApprovedDetailsPaged?page=${page}&pageSize=${pageSize}`)
        .then(r => {
            if (!r.ok) throw new Error('Failed to load approved devices');
            return r.json();
        })
        .then(result => {
            const data = result.data || [];
            const total = result.total || 0;

            Pagination.update('approved', total, page, pageSize);

            $('#span-reported-devices').text(total);
            $('#span-approved-devices').text(total);

            let rows = '';

            if (data.length === 0) {
                rows = `<tr><td colspan="10" style="text-align:center;">no approved devices available...</td></tr>`;
            } else {
                data.forEach(item => {
                    const itampatches = `
                        <select class="form-select"
                            style="border-radius:20px;cursor:pointer;text-align:center;padding:5px;font-size:12px;"
                            onchange="if(this.value){loadingStart();window.location.href=this.value;}">
                            <option value="">Manage</option>
                            <option value="/OSPatchManagement/patchdetails?type=availablepatches&id=${item.objectId}">OS Patches</option>
                            <option value="/AppManagement/appdetails?type=availablesoftwares&id=${item.objectId}">Other Software</option>
                        </select>`;

                    let itamHtml = '', plusHtml = '', linkHtml = '';

                    if (item.inITAM && item.inITAM.includes('Add')) {
                        itamHtml = `<span>add to ITAM</span>`;
                        plusHtml = `<button class='btn-info' type="button" title="Add in ITAM Registry"
                                        data-bs-toggle="tooltip" data-bs-placement="top"
                                        style="background-color:transparent;border:0;padding:5px;"
                                        data-btn-type="add" data-objectid="${item.objectId}" data-bios="${item.bios}">
                                        <i class="fas fa-plus-circle"></i></button>`;
                        linkHtml = `<button class='btn-info' type="button" title=""
                                        data-bs-toggle="tooltip" data-bs-placement="top"
                                        style="background-color:transparent;border:0;padding:5px;"
                                        data-btn-type="link-disabled">
                                        <i class="fas fa-link"></i></button>`;
                    } else if (item.inITAM && item.inITAM.includes('Link')) {
                        itamHtml = `<span>add to ITAM</span>`;
                        plusHtml = `<button class='btn-info' type="button" title=""
                                        data-bs-toggle="tooltip" data-bs-placement="top"
                                        style="background-color:transparent;border:0;padding:5px;"
                                        data-btn-type="add-disabled">
                                        <i class="fas fa-plus-circle"></i></button>`;
                        linkHtml = `
                            <form action="/Workstation/LinkWorkstation" method="post" style="display:inline;">
                                <input type="hidden" name="ObjectId"   value="${item.objectId}" />
                                <input type="hidden" name="BIOS"       value="${item.bios}" />
                                <input type="hidden" name="DeviceType" value="${item.deviceType ?? ''}" />
                                <input type="hidden" name="AssetId"    value="${item.assetId ?? ''}" />
                                <button class='btn-info' type="submit" title="${item.inITAM}"
                                    data-bs-toggle="tooltip" data-bs-placement="top"
                                    style="background-color:transparent;border:0;padding:5px;"
                                    data-btn-type="link">
                                    <i class="fas fa-link"></i></button>
                            </form>`;
                    } else {
                        itamHtml = `<span>${item.inITAM ?? ''}</span>`;
                        plusHtml = `<button class='btn-info' type="button" title=""
                                        data-bs-toggle="tooltip" data-bs-placement="top"
                                        style="background-color:transparent;border:0;padding:5px;"
                                        data-btn-type="add-disabled">
                                        <i class="fas fa-plus-circle"></i></button>`;
                        linkHtml = `<button class='btn-info' type="button" title=""
                                        data-bs-toggle="tooltip" data-bs-placement="top"
                                        style="background-color:transparent;border:0;padding:5px;"
                                        data-btn-type="link-disabled">
                                        <i class="fas fa-link"></i></button>`;
                    }

                    const statusHtml = item.status === 'Online'
                        ? `<span style="display:flex;justify-content:center;align-items:center;">
                               <box-icon style="width:8px;margin-right:5px;" name='circle' type='solid' color='#03bb07'></box-icon>
                               <span>Online</span></span>`
                        : `<span style="display:flex;justify-content:center;align-items:center;">
                               <box-icon style="width:8px;margin-right:5px;" name='circle' type='solid' color='#d80404'></box-icon>
                               <span>Offline</span></span>`;

                    rows += `
                        <tr>
                            <td>${item.systemName}</td>
                            <td>${item.loginUser}</td>
                            <td>${item.privileges}</td>
                            <td>${item.manufacturer}</td>
                            <td>${item.os}</td>
                            <td>${item.publicIP}</td>
                            <td>${statusHtml}</td>
                            <td>${itamHtml}</td>
                            <td>${itampatches}</td>
                            <td style="text-align:center;white-space:nowrap;">
                                ${plusHtml}
                                ${linkHtml}
                                <button class='btn-info' type="button" title="Display Details"
                                    data-bs-toggle="tooltip" data-bs-placement="top"
                                    style="background-color:transparent;border:0;padding:5px;"
                                    data-btn-type="info" data-objectid="${item.objectId}">
                                    <i class="fas fa-info-circle"></i></button>
                            </td>
                        </tr>`;
                });
            }

            // ── Data render ──
            $('#approvedtablebody').html(rows);

            // ── Naya data aaya — search reset karo ──
            $('#home-search-input').val('');
            _clearNoResultRow();

            setTimeout(initializeTooltips, 200);
        })
        .catch(err => console.error('Error:', err))
        .finally(() => loadingStop());
}

//function getapproved(page, pageSize) {
//    page = page || 1;
//    pageSize = pageSize || 25;

//    loadingStart();

//    fetch(`/Workstation/GetApprovedDetailsPaged?page=${page}&pageSize=${pageSize}`)
//        .then(r => {
//            if (!r.ok) throw new Error('Failed to load approved devices');
//            return r.json();
//        })
//        .then(result => {
//            const data = result.data || [];
//            const total = result.total || 0;

//            Pagination.update('approved', total, page, pageSize);

//            $('#span-reported-devices').text(total);
//            $('#span-approved-devices').text(total);

//            let rows = '';

//            if (data.length === 0) {
//                rows = `<tr><td colspan="10" style="text-align:center;">no approved devices available...</td></tr>`;
//            } else {
//                data.forEach(item => {
//                    const itampatches = `
//                        <select class="form-select"
//                            style="border-radius:20px;cursor:pointer;text-align:center;padding:5px;font-size:12px;"
//                            onchange="if(this.value){loadingStart();window.location.href=this.value;}">
//                            <option value="">Manage</option>
//                            <option value="/OSPatchManagement/patchdetails?type=availablepatches&id=${item.objectId}">OS Patches</option>
//                            <option value="/AppManagement/appdetails?type=availablesoftwares&id=${item.objectId}">Other Software</option>
//                        </select>`;

//                    let itamHtml = '', plusHtml = '', linkHtml = '';

//                    if (item.inITAM && item.inITAM.includes('Add')) {
//                        itamHtml = `<span>add to ITAM</span>`;
//                        plusHtml = `<button class='btn-info' type="button" title="Add in ITAM Registry"
//                                        data-bs-toggle="tooltip" data-bs-placement="top"
//                                        style="background-color:transparent;border:0;padding:5px;"
//                                        data-btn-type="add" data-objectid="${item.objectId}" data-bios="${item.bios}">
//                                        <i class="fas fa-plus-circle"></i></button>`;
//                        linkHtml = `<button class='btn-info' type="button" title=""
//                                        data-bs-toggle="tooltip" data-bs-placement="top"
//                                        style="background-color:transparent;border:0;padding:5px;"
//                                        data-btn-type="link-disabled">
//                                        <i class="fas fa-link"></i></button>`;
//                    } else if (item.inITAM && item.inITAM.includes('Link')) {
//                        itamHtml = `<span>add to ITAM</span>`;
//                        plusHtml = `<button class='btn-info' type="button" title=""
//                                        data-bs-toggle="tooltip" data-bs-placement="top"
//                                        style="background-color:transparent;border:0;padding:5px;"
//                                        data-btn-type="add-disabled">
//                                        <i class="fas fa-plus-circle"></i></button>`;
//                        linkHtml = `
//                            <form action="/Workstation/LinkWorkstation" method="post" style="display:inline;">
//                                <input type="hidden" name="ObjectId"   value="${item.objectId}" />
//                                <input type="hidden" name="BIOS"       value="${item.bios}" />
//                                <input type="hidden" name="DeviceType" value="${item.deviceType ?? ''}" />
//                                <input type="hidden" name="AssetId"    value="${item.assetId ?? ''}" />
//                                <button class='btn-info' type="submit" title="${item.inITAM}"
//                                    data-bs-toggle="tooltip" data-bs-placement="top"
//                                    style="background-color:transparent;border:0;padding:5px;"
//                                    data-btn-type="link">
//                                    <i class="fas fa-link"></i></button>
//                            </form>`;
//                    } else {
//                        itamHtml = `<span>${item.inITAM ?? ''}</span>`;
//                        plusHtml = `<button class='btn-info' type="button" title=""
//                                        data-bs-toggle="tooltip" data-bs-placement="top"
//                                        style="background-color:transparent;border:0;padding:5px;"
//                                        data-btn-type="add-disabled">
//                                        <i class="fas fa-plus-circle"></i></button>`;
//                        linkHtml = `<button class='btn-info' type="button" title=""
//                                        data-bs-toggle="tooltip" data-bs-placement="top"
//                                        style="background-color:transparent;border:0;padding:5px;"
//                                        data-btn-type="link-disabled">
//                                        <i class="fas fa-link"></i></button>`;
//                    }

//                    const statusHtml = item.status === 'Online'
//                        ? `<span style="display:flex;justify-content:center;align-items:center;">
//                               <box-icon style="width:8px;margin-right:5px;" name='circle' type='solid' color='#03bb07'></box-icon>
//                               <span>Online</span></span>`
//                        : `<span style="display:flex;justify-content:center;align-items:center;">
//                               <box-icon style="width:8px;margin-right:5px;" name='circle' type='solid' color='#d80404'></box-icon>
//                               <span>Offline</span></span>`;

//                    rows += `
//                        <tr>
//                            <td>${item.systemName}</td>
//                            <td>${item.loginUser}</td>
//                            <td>${item.privileges}</td>
//                            <td>${item.manufacturer}</td>
//                            <td>${item.os}</td>
//                            <td>${item.publicIP}</td>
//                            <td>${statusHtml}</td>
//                            <td>${itamHtml}</td>
//                            <td>${itampatches}</td>
//                            <td style="text-align:center;white-space:nowrap;">
//                                ${plusHtml}
//                                ${linkHtml}
//                                <button class='btn-info' type="button" title="Display Details"
//                                    data-bs-toggle="tooltip" data-bs-placement="top"
//                                    style="background-color:transparent;border:0;padding:5px;"
//                                    data-btn-type="info" data-objectid="${item.objectId}">
//                                    <i class="fas fa-info-circle"></i></button>
//                            </td>
//                        </tr>`;
//                });
//            }

//            $('#approvedtablebody').html(rows);
//            setTimeout(initializeTooltips, 200);
//        })

//        .catch(err => console.error('Error:', err))
//        .finally(() => loadingStop());
//}

// ─── Load unapproved devices (paged) ─────────────────────────────────────────

function getunapproved(page, pageSize) {
    page = page || 1;
    pageSize = pageSize || 25;

    loadingStart();

    fetch(`/Workstation/GetUnApprovedDetailsPaged?page=${page}&pageSize=${pageSize}`)
        .then(r => {
            if (!r.ok) throw new Error('Failed to load unapproved devices');
            return r.json();
        })
        .then(result => {
            const data = result.data || [];
            const total = result.total || 0;

            Pagination.update('unapproved', total, page, pageSize);

            const approvedCount = parseInt($('#span-approved-devices').text()) || 0;
            $('#span-reported-devices').text(approvedCount + total);
            $('#span-unapproved-devices').text(total);

            let rows = '';

            if (!data || data.length === 0) {
                rows = `<tr><td colspan="9" style="text-align:center;">no unapproved devices available...</td></tr>`;
            } else {
                data.forEach(item => {
                    rows += `
                        <tr>
                            <td>${item.systemName}</td>
                            <td>${item.loginUser}</td>
                            <td>${item.domain}</td>
                            <td>${item.privileges}</td>
                            <td>${item.manufacturer}</td>
                            <td>${item.os}</td>
                            <td>${item.publicIP}</td>
                            <td>${item.inITAM === true ? 'Yes' : 'No'}</td>
                            <td style="text-align:center;white-space:nowrap;">
                                <button class='btn-unapprove-click' type="button" title="Approve"
                                    data-bs-toggle="tooltip" data-bs-placement="top"
                                    style="background-color:transparent;border:0;padding:5px;"
                                    data-btn-type="approve" data-objectid="${item.id}">
                                    <i class="fas fa-check"></i></button>
                                <button class='btn-unapprove-click' type="button" title="Delete"
                                    data-bs-toggle="tooltip" data-bs-placement="top"
                                    style="background-color:transparent;border:0;padding:5px;"
                                    data-btn-type="delete" data-objectid="${item.id}">
                                    <i class="fas fa-trash"></i></button>
                            </td>
                        </tr>`;
                });
            }

            // ── Data render ──
            $('#unapprovedtablebody').html(rows);

            // ── Naya data aaya — search reset karo ──
            $('#home-search-input').val('');
            _clearNoResultRow();

            setTimeout(initializeTooltips, 200);
        })
        .catch(err => console.error('Error:', err))
        .finally(() => loadingStop());
}

//function getunapproved(page, pageSize) {
//    page = page || 1;
//    pageSize = pageSize || 25;

//    loadingStart();

//    fetch(`/Workstation/GetUnApprovedDetailsPaged?page=${page}&pageSize=${pageSize}`)
//        .then(r => {
//            if (!r.ok) throw new Error('Failed to load unapproved devices');
//            return r.json();
//        })
//        .then(result => {
//            const data = result.data || [];
//            const total = result.total || 0;

//            Pagination.update('unapproved', total, page, pageSize);

//            const approvedCount = parseInt($('#span-approved-devices').text()) || 0;
//            $('#span-reported-devices').text(approvedCount + total);
//            $('#span-unapproved-devices').text(total);

//            let rows = '';
//            if (!data || data.length === 0) {
//                rows = `<tr><td colspan="9" style="text-align:center;">no unapproved devices available...</td></tr>`;
//            } else {
//                data.forEach(item => {
//                    rows += `
//                        <tr>
//                            <td>${item.systemName}</td>
//                            <td>${item.loginUser}</td>
//                            <td>${item.domain}</td>
//                            <td>${item.privileges}</td>
//                            <td>${item.manufacturer}</td>
//                            <td>${item.os}</td>
//                            <td>${item.publicIP}</td>
//                            <td>${item.inITAM === true ? 'Yes' : 'No'}</td>
//                            <td style="text-align:center;white-space:nowrap;">
//                                <button class='btn-unapprove-click' type="button" title="Approve"
//                                    data-bs-toggle="tooltip" data-bs-placement="top"
//                                    style="background-color:transparent;border:0;padding:5px;"
//                                    data-btn-type="approve" data-objectid="${item.id}">
//                                    <i class="fas fa-check"></i></button>
//                                <button class='btn-unapprove-click' type="button" title="Delete"
//                                    data-bs-toggle="tooltip" data-bs-placement="top"
//                                    style="background-color:transparent;border:0;padding:5px;"
//                                    data-btn-type="delete" data-objectid="${item.id}">
//                                    <i class="fas fa-trash"></i></button>
//                            </td>
//                        </tr>`;
//                });
//            }

//            $('#unapprovedtablebody').html(rows);
//            setTimeout(initializeTooltips, 200);
//        })
//        .catch(err => console.error('Error:', err))
//        .finally(() => loadingStop());
//}

// ─── Helpers ──────────────────────────────────────────────────────────────────
function reloadWithType(newType) {
    const url = new URL(window.location.href);
    url.searchParams.set('type', newType);
    window.location.href = url.toString();
}

function Details(id, b) {
    $.ajax({
        type: 'GET',
        url: 'Workstation/AddInRegistry?id=' + id + '&bios=' + b,
        beforeSend: function () { loadingStart(); },
        success: function (res) {
            $('#form-modal .modal-body').empty().html(res);
            new bootstrap.Modal(document.getElementById('form-modal')).show();
        },
        complete: function () { loadingStop(); }
    });
}

function fetchDeviceInfo(id) {
    $.ajax({
        type: 'GET',
        url: 'Workstation/GetDeviceInfoInPV?ID=' + id,
        beforeSend: function () { loadingStart(); },
        success: function (res) {
            $('#form-modal .modal-body').empty().html(res);
            new bootstrap.Modal(document.getElementById('form-modal')).show();
        },
        complete: function () { loadingStop(); }
    });
}

function postapproveDevice(deviceId) {
    $.ajax({
        url: 'Workstation/ApproveDevice', type: 'GET', data: { ID: deviceId },
        beforeSend: function () { loadingStart(); },
        success: function (response) {
            if (response) {
                const s = Pagination.getState('unapproved');
                getunapproved(s.page, s.pageSize);
            } else {
                alert('Failed to approve device.');
            }
        },
        error: function () { alert('An error occurred while approving device.'); },
        complete: function () { loadingStop(); }
    });
}

function postdeleteDevice(deviceId) {
    $.ajax({
        url: 'Workstation/DeleteDevice', type: 'GET', data: { ID: deviceId },
        beforeSend: function () { loadingStart(); },
        success: function (response) {
            if (response) {
                const s = Pagination.getState('unapproved');
                getunapproved(s.page, s.pageSize);
            } else {
                alert('Failed to delete device.');
            }
        },
        error: function () { alert('An error occurred while deleting device.'); },
        complete: function () { loadingStop(); }
    });
}

// ─── Button click handlers ────────────────────────────────────────────────────
$(document).on('click', '.btn-info', function () {
    const btnType = $(this).data('btn-type');
    const objectId = $(this).data('objectid');
    if (btnType === 'add') {
        Details(objectId, $(this).data('bios'));
        $('#detailLabel').empty().text('Add Device In Registry');
    }
    if (btnType === 'info') {
        fetchDeviceInfo(objectId);
        $('#detailLabel').empty().text('Device Details');
    }
});

$(document).on('click', '.btn-unapprove-click', function () {
    const btnType = $(this).data('btn-type');
    const objectId = $(this).data('objectid');
    if (btnType === 'approve') postapproveDevice(objectId);
    if (btnType === 'delete') postdeleteDevice(objectId);
});

// ─── Patch status polling ─────────────────────────────────────────────────────
const patchPollers = {};
function pollPatchStatus(SystemID) {
    if (patchPollers[SystemID]) return;
    patchPollers[SystemID] = setInterval(() => {
        fetch(`/OSPatchManagement/GetPatchStatus?objId=${SystemID}`)
            .then(r => r.json())
            .then(patches => {
                if (!Array.isArray(patches) || !patches.length) return;
                patches.forEach(p => {
                    const el = document.getElementById(`status-${p.patchId}`);
                    if (!el) return;
                    let text = 'Installing', color = 'orange';
                    if (p.status === '1') { text = p.reason; color = 'green'; }
                    else if (p.status === '2') { text = p.reason && p.reason.trim() !== '' ? p.reason : 'Installation Failed'; color = 'red'; }
                    else if (p.status === '0') { text = 'Installing'; color = 'blue'; }
                    el.innerText = text;
                    el.style.color = color;
                    const cb = document.getElementById(`patch-${SystemID}-${p.patchId}`);
                    if (cb && (p.status === '0' || p.status === '1')) cb.disabled = true;
                });
            })
            .catch(err => console.error('Patch polling error:', err));
    }, 10000);
}

// ─── Home search helper ───────────────────────────────────────────────────────
function _clearNoResultRow() {
    $('.home-no-result-row').remove();
}




//////// This is my Old Working Code
///// <reference path="../../lib/jquery/dist/jquery.min.js" />
///// <reference path="../../plugins/popper/popper.min.js" />

//function initializeTooltips() {
//    // Dispose any existing tooltips first
//    var existingTooltips = document.querySelectorAll('[data-bs-toggle="tooltip"]');
//    existingTooltips.forEach(function (element) {
//        var tooltip = bootstrap.Tooltip.getInstance(element);
//        if (tooltip) {
//            tooltip.dispose();
//        }
//    });

//    // Initialize new tooltips
//    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
//    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
//        return new bootstrap.Tooltip(tooltipTriggerEl);
//    });

//    console.log('Tooltips initialized:', tooltipList.length); // Debug log
//}
//$(document).ready(function () {
//    getapproved();

//    $("#approvedDeviceSearch").on("keyup", function () {
//        var value = $(this).val().toLowerCase();
//        $("tbody tr").filter(function () {
//            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
//        });
//    });

//    $('#txt_patch_search').on('keyup', function () {
//        var searchValue = $(this).val().toLowerCase().trim();
//        var targetTable = $(this).data('target-table');

//        if (targetTable) {
//            // Remove existing "no data found" row if present
//            $('#' + targetTable + ' tbody tr.no-data-row').remove();

//            if (searchValue === '') {
//                // Show all rows if search is empty
//                $('#' + targetTable + ' tbody tr').show();
//            } else {
//                var visibleCount = 0;

//                $('#' + targetTable + ' tbody tr').each(function () {
//                    var rowText = $(this).text().toLowerCase();
//                    var isVisible = rowText.indexOf(searchValue) > -1;
//                    $(this).toggle(isVisible);

//                    if (isVisible) {
//                        visibleCount++;
//                    }
//                });

//                // If no rows are visible, show "No data found" message
//                if (visibleCount === 0) {
//                    var colCount = $('#' + targetTable + ' thead th').length;
//                    var noDataRow = '<tr class="no-data-row"><td colspan="' + colCount + '" style="text-align: center; padding:10px 20px; color: #6c757d;">no data found...</td></tr>';
//                    $('#' + targetTable + ' tbody').append(noDataRow);
//                }
//            }
//        }
//    });

//    // Clear search
//    $('#txt_patch_search').on('search', function () {
//        if ($(this).val() === '') {
//            var targetTable = $(this).data('target-table');
//            $('#' + targetTable + ' tbody tr.no-data-row').remove();
//            $('#' + targetTable + ' tbody tr').show();
//        }
//    });


//});
//$(document).on('click', '.device-model-nav .nav-link', function () {

//    // Activate selected tab
//    $(this).closest('ul').find('.nav-link').removeClass('active');
//    $(this).addClass('active');

//    const type = $(this).data('type');
//    const tabId = $(this).data('tabid');

//    // Show container if exists
//    if (tabId) {
//        $('.tab-container').hide();
//        $('#' + tabId).show();
//    }

//    // Workstation controller
//    if (type === "approved") getapproved();
//    if (type === "unapproved") getunapproved();

//    // Patch controller
//    if (type === "availablepatches" || type === "installedpatches") {
//        reloadWithType(type);
//    }

//    // Software controller
//    if (type === "availablesoftwares" || type === "installedsoftwares") {
//        reloadWithType(type);
//    }
//});
//function reloadWithType(newType) {
//    const url = new URL(window.location.href);

//    // update or add the "type" parameter
//    url.searchParams.set("type", newType);

//    // reload with updated query string
//    window.location.href = url.toString();
//}
//function getapproved() {
//    loadingStart();
//    fetch(`/Workstation/GetApprovedDetails`)
//        .then(response => {
//            if (!response.ok) {
//                throw new Error("Failed to load approved devices");
//            }
//            return response.json();
//        })
//        .then(data => {

//            console.log(data);

//            let rows = "";

//            if (data.length === 0) {
//                rows = `
//                                                <tr>
//                                                    <td colspan="10">no approved devices available...</td>
//                                                </tr>
//                                            `;
//            } else {
//                data.forEach(item => {
//                    let itampatches = `
//                                            <select class="form-select"
//                                                        style="border-radius:20px; cursor:pointer; text-align:center; padding:5px; font-size:12px;"
//                                                        onchange="if(this.value) loadingStart(); window.location.href=this.value;">
//                                                    <option style="cursor:pointer;" value="">Manage</option>
//                                                    <option style="cursor:pointer;" value="/OSPatchManagement/patchdetails?type=availablepatches&id=${item.objectId}">OS Patches</option>
//                                                    <option style="cursor:pointer;" value="/AppManagement/appdetails?type=availablesoftwares&id=${item.objectId}">Other Software</option>
//                                                </select>
//                                        `;

//                    let itamHtml = "";
//                    let plusHtml = "";
//                    let linkHtml = "";

//                    if (item.inITAM && item.inITAM.includes("Add")) {
//                        itamHtml = `<span>add to ITAM</span>`;
//                        plusHtml = `<button class='btn-info' type="button" title="Add in ITAM Registry" data-bs-toggle="tooltip" data-bs-placement="top"  style="background-color:transparent;border:0; padding:5px;" data-btn-type="add" data-objectid=${item.objectId} data-bios="${item.bios}" ><i class="fas fa-plus-circle"></i></button>`;
//                        linkHtml = `<button class='btn-info' type="button" title="" data-bs-toggle="tooltip" data-bs-placement="top" style="background-color:transparent;border:0; padding:5px;" data-btn-type="link-disabled" data-objectid=${item.objectId}><i class="fas fa-link"></i></button>`;
//                    }
//                    else if (item.inITAM && item.inITAM.includes("Link")) {
//                        itamHtml = `<span>add to ITAM</span>`;
//                        plusHtml = `<button class='btn-info' type="button" title="" data-bs-toggle="tooltip" data-bs-placement="top" style="background-color:transparent;border:0; padding:5px;" data-btn-type="add-disabled" data-objectid=${item.objectId}><i class="fas fa-plus-circle"></i></button>`;
//                        linkHtml = `
//                                    <form action="/Workstation/LinkWorkstation"
//                                                                      method="post"
//                                                                      style="display:inline;">
//                                                                    <input type="hidden" name="ObjectId" value="${item.objectId}" />
//                                                                    <input type="hidden" name="BIOS" value="${item.bios}" />
//                                                                    <input type="hidden" name="DeviceType" value="${item.deviceType ?? ''}" />
//                                                                    <input type="hidden" name="AssetId" value="${item.assetId ?? ''}" />
//                                                                    <button class='btn-info' type="submit" title="${item.inITAM}" data-bs-toggle="tooltip" data-bs-placement="top"  style="background-color:transparent;border:0; padding:5px;" data-btn-type="link"><i class="fas fa-link"></i></button>
//                                    </form>
//                                    `;



//                    }
//                    else {
//                        itamHtml = `<span>${item.inITAM ?? ''}</span>`;
//                        plusHtml = `<button class='btn-info' type="button" title="" data-bs-toggle="tooltip" data-bs-placement="top" style="background-color:transparent;border:0; padding:5px;" data-btn-type="add-disabled" data-objectid=${item.objectId}><i class="fas fa-plus-circle"></i></button>`;
//                        linkHtml = `<button class='btn-info' type="button" title="" data-bs-toggle="tooltip" data-bs-placement="top" style="background-color:transparent;border:0; padding:5px;" data-btn-type="link-disabled" data-objectid=${item.objectId}><i class="fas fa-link"></i></button>`;
//                    }

//                    let statusHtml = "";
//                    if (item.status === 'Online') {
//                        statusHtml = `
//                                                                                <span style="display:flex; justify-content: center; align-items: center">
//                                                                                   <box-icon style="width:8px; margin-right: 5px" name='circle' type='solid' color='#03bb07'></box-icon>
//                                                                                   <span>Online</span>
//                                                                                </span>
//                                                                         `;
//                    } else {
//                        statusHtml = `
//                                                                            <span style="display:flex; justify-content: center; align-items: center">
//                                                                                                    <box-icon style="width:8px; margin-right: 5px" name='circle' type='solid' color='#d80404'></box-icon>
//                                                                                                    <span>Offline</span>
//                                                                                                </span>
//                                                                       `;
//                    }

//                    rows += `
//                                                            <tr>
//                                                                <td>${item.systemName}</td>
//                                                                <td>${item.loginUser}</td>
//                                                                <td>${item.privileges}</td>
//                                                                <td>${item.manufacturer}</td>
//                                                                <td>${item.os}</td>
//                                                                <td>${item.publicIP}</td>
//                                                                <td>${statusHtml}</td>
//                                                                <td>${itamHtml}</td>
//                                                                <td>${itampatches}</td>
//                                                                <td style='text-align:center; white-space:nowrap;'>
//                                                                    ${plusHtml}
//                                                                    ${linkHtml}
//                                                                    <button class='btn-info' type="button"  title="Display Details" data-bs-toggle="tooltip" data-bs-placement="top" style="background-color:transparent;border:0; padding:5px;" data-btn-type="info" data-objectid=${item.objectId}><i class="fas fa-info-circle"></i></button>
//                                                                </td>
//                                                            </tr>
//                                                        `;
//                });


//            }

//            // clear + append
//            $('#approvedtablebody').html(rows);
//            // Initialize tooltips after rendering
//            setTimeout(function () {
//                initializeTooltips();
//            }, 200);
//        })
//        .catch(error => {
//            console.error("Error:", error);
//        })
//        .finally(() => {
//            loadingStop();
//        });
//}
//function getunapproved() {
//    loadingStart();
//    fetch(`/Workstation/GetUnApprovedDetails`)
//        .then(response => {
//            if (!response.ok) {
//                throw new Error("Failed to load unapproved devices");
//            }
//            return response.json();
//        })
//        .then(data => {

//            console.log(data);

//            let rows = "";

//            if (data.length === 0) {
//                rows = `
//                        <tr>
//                            <td colspan="9">no unapproved devices available...</td>
//                        </tr>
//                       `;
//            } else {
//                data.forEach(item => {
//                    rows += `
//                                                            <tr>
//                                                                <td>${item.systemName}</td>
//                                                                <td>${item.loginUser}</td>
//                                                                <td>${item.domain}</td>
//                                                                <td>${item.privileges}</td>
//                                                                <td>${item.manufacturer}</td>
//                                                                <td>${item.os}</td>
//                                                                <td>${item.publicIP}</td>
//                                                                <td>${item.inITAM===true?'Yes':'No'}</td>
//                                                                <td style='text-align:center; white-space:nowrap;'>
//                                                                    <button class='btn-unapprove-click' type="button" title="Approve" data-bs-toggle="tooltip" data-bs-placement="top" style="background-color:transparent;border:0; padding:5px;" data-btn-type="approve" data-objectid=${item.id}><i class="fas fa-check"></i></button>
//                                                                    <button class='btn-unapprove-click' type="button" title="Delete" data-bs-toggle="tooltip" data-bs-placement="top" style="background-color:transparent;border:0; padding:5px;" data-btn-type="delete" data-objectid=${item.id}><i class="fas fa-trash"></i></button>
//                                                                </td>
//                                                            </tr>
//                                                        `;
//                });
//            }
//            $('#unapprovedtablebody').html(rows);
//            // Initialize tooltips after rendering
//            setTimeout(function () {
//                initializeTooltips();
//            }, 200);
//        })
//        .catch(error => {
//            console.error("Error:", error);
//        })
//        .finally(() => {
//            loadingStop();
//        });
//}
//function Details(id, b) {
//    $.ajax({
//        type: "GET",
//        url: 'Workstation/AddInRegistry?id=' + id + '&bios=' + b,
//        beforeSend: function () {
//            loadingStart(); // Call before sending the request
//        },
//        success: function (res) {
//            $("#form-modal .modal-body").empty().html(res);
//            /*$("#form-modal").modal('show');*/
//            const modal = new bootstrap.Modal(document.getElementById('form-modal'));
//            modal.show();
//        },
//        complete: function () {
//            loadingStop(); // Always called after success or error
//        }
//    });
//}
//function fetchDeviceInfo(id) {
//    $.ajax({
//        type: "GET",
//        url: 'Workstation/GetDeviceInfoInPV?ID=' + id,
//        beforeSend: function () {
//            loadingStart(); // Call before sending the request
//        },
//        success: function (res) {
//            $("#form-modal .modal-body").empty().html(res);
//            /*$("#form-modal").modal('show');*/
//            const modal = new bootstrap.Modal(document.getElementById('form-modal'));
//            modal.show();
//        },
//        complete: function () {
//            loadingStop(); // Always called after success or error
//        }
//    });
//}
//function postapproveDevice(deviceId) {
//    $.ajax({
//        url: 'Workstation/ApproveDevice',
//        type: 'GET',
//        data: { ID: deviceId },
//        beforeSend: function () {
//            loadingStart();
//        },
//        success: function (response) {
//            if (response) {
//                getunapproved();
//            } else {
//                alert("Failed to approve device.");
//            }
//        },
//        error: function () {
//            alert("An error occurred while approving device.");
//        },
//        complete: function () {
//            loadingStop(); // Always called after success or error
//        }
//    });
//}
//function postdeleteDevice(deviceId) {
//    $.ajax({
//        url: 'Workstation/DeleteDevice',
//        type: 'GET',
//        data: { ID: deviceId },
//        beforeSend: function () {
//            loadingStart();
//        },
//        success: function (response) {
//            if (response) {
//                getunapproved();
//            } else {
//                alert("Failed to delete device.");
//            }
//        },
//        error: function () {
//            alert("An error occurred while deleting device.");
//        },
//        complete: function () {
//            loadingStop(); // Always called after success or error
//        }
//    });
//}
////$(document).on('click', '.btn-add-itam', function () {
////    const objectId = $(this).data('objectid');
////    const bios = $(this).data('bios');
////    Details(objectId, bios);
////    $('#detailLabel').empty().text('Add Device In Registry');
////});
//$(document).on('click', '.btn-info', function () {
//    const btnType = $(this).data('btn-type');
//    const objectId = $(this).data('objectid');
//    if (btnType === "add") {
//        const objectId = $(this).data('objectid');
//        const bios = $(this).data('bios');
//        Details(objectId, bios);
//        $('#detailLabel').empty().text('Add Device In Registry');
//    }
//    if (btnType === "info") {
//        fetchDeviceInfo(objectId);
//        $('#detailLabel').empty().text('Device Details');
//    }
//});
//$(document).on('click', '.btn-unapprove-click', function () {
//    const btnType = $(this).data('btn-type');
//    const objectId = $(this).data('objectid');
//    if (btnType === "approve") {
//        postapproveDevice(objectId);
//    }
//    if (btnType === "delete") {
//        postdeleteDevice(objectId);
//    }
//});

//const patchPollers = {};
//function pollPatchStatus(SystemID) {

//    if (patchPollers[SystemID]) return; // prevent duplicate pollers

//    patchPollers[SystemID] = setInterval(() => {

//        fetch(`/OSPatchManagement/GetPatchStatus?objId=${SystemID}`)
//            .then(r => r.json())
//            .then(patches => {

//                if (!Array.isArray(patches) || patches.length === 0)
//                    return;

//                patches.forEach(p => {

//                    const statusElement = document.getElementById(`status-${p.patchId}`);
//                    if (!statusElement) return;

//                    let statusText = "Installing"; // default safe status
//                    let color = "orange";

//                    if (p.status === "1") {
//                        statusText = p.reason;
//                        color = "green";
//                    }
//                    else if (p.status === "2") {
//                        statusText = p.reason && p.reason.trim() !== ""
//                            ? p.reason
//                            : "Installation Failed";
//                        color = "red";
//                    }
//                    else if (p.status === "0") {
//                        statusText = "Installing";
//                        color = "blue";
//                    }
//                    else {
//                        // agent picked but not finished
//                        statusText = "Installing";
//                        color = "orange";
//                    }

//                    statusElement.innerText = statusText;
//                    statusElement.style.color = color;

//                    // disable checkbox when installing/success
//                    const checkbox = document.getElementById(`patch-${SystemID}-${p.patchId}`);
//                    if (checkbox && (p.status === "0" || p.status === "1")) {
//                        checkbox.disabled = true;
//                    }

//                });

//            })
//            .catch(err => console.error("Patch polling error:", err));

//    }, 10000);
//}
