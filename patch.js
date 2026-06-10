$(document).on('click', '#patchTabs .nav-link', function () {

    // toggle active class
    $('#patchTabs .nav-link').removeClass('active');
    $(this).addClass('active');

    const type = $(this).data('type');

    if (type === 'installed') {
        $('#btn-install').hide();
        //window.location.href = "OSPatchManagement/PatchDetails";
    } else {
        $('#btn-install').show().prop('disabled', true).text('Install');
    }

});

$(document).on('change', '#chk-select-all', function () {

    let modal = $('#patchModal')
    let isChecked = $(this).is(':checked');

    // Check only enabled checkboxes
    $('.chk-patch').not(':disabled').prop('checked', isChecked);

    updateInstallButton();

    // Open modal if at least one checkbox is checked
    const checked = $('.chk-patch:checked').length;
    
    if (checked == 0) {
        modal.hide();
    }
});
$(document).on('change', '.chk-patch', function () {

    const total = $('.chk-patch').not(':disabled').length; // only enabled
    const checked = $('.chk-patch:checked').length;
    let modal = $('#patchModal')

    // Sync select-all (only for enabled checkboxes)
    $('#chk-select-all').prop('checked', total === checked);

    updateInstallButton();    
    if (checked == 0) {
        modal.hide();
    }
});

$(document).on('click', '#btn-install-later', function () {
    const checkedCount = $('.chk-patch:checked').length;
    let modal = $('#patchModal')
    $('#btn-install')
        .prop('disabled', true)
        .text(`Install now (${checkedCount})`);
    modal.show();
});

// Enable/Disable install + update count
function updateInstallButton() {
    const checkedCount = $('.chk-patch:checked').length;
    let modal = $('#patchModal');

    if (checkedCount > 0) {

        if (modal.is(':hidden')) {
            $('#btn-install')
                .prop('disabled', false)
                .text(`Install now (${checkedCount})`);
        } else {
            $('#btn-install')
                .prop('disabled', true)
                .text(`Install now (${checkedCount})`);
        }

        $('#btn-install-later')
            .prop('disabled', false)
            .text(`Install later (${checkedCount})`);

        $('#installDateTime').val('');
    } else {
        $('#btn-install')
            .prop('disabled', true)
            .text('Install now');

        $('#btn-install-later')
            .prop('disabled', true)
            .text('Install later');
    }
}
//function updateInstallButton() {
//    const checkedCount = $('.chk-patch:checked').length;

//    if (checkedCount > 0) {
//        $('#btn-install')
//            .prop('disabled', false)
//            .text(`Install now (${checkedCount})`);
//        $('#btn-install-later')
//            .prop('disabled', false)
//            .text(`Install later (${checkedCount})`);
//        $('#installDateTime').val('');
//    } else {
//        $('#btn-install')
//            .prop('disabled', true) //true
//            .text('Install now');
//        $('#btn-install-later')
//            .prop('disabled', true) //true
//            .text('Install later');
//    }
//}