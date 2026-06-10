/// <reference path="../../lib/jquery/dist/jquery.js" />

$(document).ready(function () {
    let $gridIcon = $('.gridIcon');
    let $menu = $('.MenucollapseInner');

    $gridIcon.click(function (e) {
        e.stopPropagation(); // Prevent the click from propagating to the document
        if ($menu.css('visibility') === 'hidden') {
            $menu.css('visibility', 'visible');
        } else {
            $menu.css('visibility', 'hidden');
        }
    });

    $(document).click(function (e) {
        if (!$menu.is(e.target) && !$menu.has(e.target).length) {
            $menu.css('visibility', 'hidden');
        }
    });

    //Menu Expansion ( grid-3x3)
    $('.MenucollapseInnerMenuListItem').on('mouseover', function () {
        let span = $(this).find('span');
        let hoverText = span.attr('data-hover'); // Get the data-hover value
        span.text(hoverText); // Set the text to the hover value
        span.attr('tooltip', hoverText);
    });

    // Mouseout event on span elements
    $('.MenucollapseInnerMenuListItem').on('mouseout', function () {
        let span = $(this).find('span');
        let originalText = span.attr('data'); // Get the original data value
        span.text(originalText); // Set the text back to the original value
        span.attr('tooltip', originalText);
    });

    //// Harmburg
    //$('.nav-toggle').click(function (e) {
    //    e.preventDefault();
    //    $("html").toggleClass("openNav");
    //    $(".nav-toggle").toggleClass("active");

    //});

});

//Password Icon
$("i[data-op=pwd]").click(function () {
    let $targetElement = $(this).data('target');
    let $iclass = $(this).attr('class');
    let $eye = 'bi bi-eye';
    let $eyeslash = 'bi bi-eye-slash';
    $(this).removeAttr('class');
    $(this).removeAttr('data-target-icon')

    $iclass == $eye ? $(this).attr('class', $eyeslash) : $(this).attr('class', $eye);

    if ($("#" + $targetElement).attr('type') === 'text') {
        $("#" + $targetElement).removeAttr('type');
        $("#" + $targetElement).attr('type', 'password');
    } else {
        $("#" + $targetElement).removeAttr('type');
        $("#" + $targetElement).attr('type', 'text');
    }
});

//Sidebar menu
$('.primary-nav').hover(function () {
    $(this).css("width", "250px");
    $("#MainBodys").addClass("menu_hover");
}, function () {
    $(this).css("width", "");
    $("#MainBodys").removeClass("menu_hover");
    $('#Workstation-submenu').slideUp();
});

$('#WorkStationDropdownLink').hover(function () {
    let $id = $(this).data('dropdown-list');
    $('#' + $id).slideDown();
});

$('#Workstation-submenu').on('mouseleave', function () {
    $(this).slideUp();
});


//let $timeout_menu;

//// Handle mouse enter
//$('#WorkStationDropdownLink, #Workstation-submenu').on('mouseenter', function () {
//    clearTimeout($timeout_menu);
//    $('#Workstation-submenu').stop(true, true).slideDown('fast');
//});

//// Handle mouse leave
//$('#WorkStationDropdownLink, #Workstation-submenu').on('mouseleave', function () {
//    $timeout_menu = setTimeout(function () {
//        $('#Workstation-submenu').stop(true, true).slideUp('fast');
//    }, 200); // Delay for better UX
//});