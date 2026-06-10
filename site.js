const body = document.querySelector("body");
const darkLight = document.querySelector("#darkLight");
//const sidebar = document.querySelector(".sidebar");
//const submenuItems = document.querySelectorAll(".submenu_item");
//const sidebarOpen = document.querySelector("#sidebarOpen");
//const sidebarClose = document.querySelector(".collapse_sidebar");
//const sidebarExpand = document.querySelector(".expand_sidebar");
//sidebarOpen.addEventListener("click", () => sidebar.classList.toggle("close"));

//sidebarClose.addEventListener("click", () => {
//    sidebar.classList.add("close", "hoverable");
//});
//sidebarExpand.addEventListener("click", () => {
//    sidebar.classList.remove("close", "hoverable");
//});

//sidebar.addEventListener("mouseenter", () => {
//    if (sidebar.classList.contains("hoverable")) {
//        sidebar.classList.remove("close");
//    }
//});
//sidebar.addEventListener("mouseleave", () => {
//    if (sidebar.classList.contains("hoverable")) {
//        sidebar.classList.add("close");
//    }
//});

//darkLight.addEventListener("click", () => {
//    body.classList.toggle("dark");
//    if (body.classList.contains("dark")) {
//        document.setI;
//        darkLight.classList.replace("bx-sun", "bx-moon");
//    } else {
//        darkLight.classList.replace("bx-moon", "bx-sun");
//    }
//});

//submenuItems.forEach((item, index) => {
//    item.addEventListener("click", () => {
//        item.classList.toggle("show_submenu");
//        submenuItems.forEach((item2, index2) => {
//            if (index !== index2) {
//                item2.classList.remove("show_submenu");
//            }
//        });
//    });
//});

//if (window.innerWidth < 768) {
//    sidebar.classList.add("close");
//} else {
//    sidebar.classList.remove("close");
//}





//Sorting Start

function mergeSort(arr, comparator) {
    if (arr.length <= 1) {
        return arr;
    }

    const middle = Math.floor(arr.length / 2);
    const left = arr.slice(0, middle);
    const right = arr.slice(middle);

    return merge(
        mergeSort(left, comparator),
        mergeSort(right, comparator),
        comparator
    );
}

function merge(left, right, comparator) {
    let result = [];
    let leftIndex = 0;
    let rightIndex = 0;

    while (leftIndex < left.length && rightIndex < right.length) {
        if (comparator(left[leftIndex], right[rightIndex]) <= 0) {
            result.push(left[leftIndex]);
            leftIndex++;
        } else {
            result.push(right[rightIndex]);
            rightIndex++;
        }
    }

    return result.concat(left.slice(leftIndex)).concat(right.slice(rightIndex));
}

let sortDirection = 'ascending';

function compareValues(a, b) {
    if (!isNaN(a) && !isNaN(b)) {
        return parseFloat(a) - parseFloat(b);
    }

    // Check if the values are dates
    const dateRegex = /^(0?[1-9]|[12][0-9]|3[01])[-\/](0?[1-9]|1[012])[-\/]\d{4}$/; // Matches formats like DD-MM-YYYY, DD/MM/YYYY, MM/DD/YYYY
    if (dateRegex.test(a) && dateRegex.test(b)) {
        // Parse dates
        const dateA = parseDate(a);
        const dateB = parseDate(b);
        return dateA - dateB;
    }

    // Check if the values are datetimes
    const datetimeRegex = /^(0?[1-9]|[12][0-9]|3[01])[-\/](0?[1-9]|1[012])[-\/]\d{4} (\d{2}):(\d{2}):(\d{2})$/; // Matches formats like DD-MM-YYYY HH:MM:SS
    if (datetimeRegex.test(a) && datetimeRegex.test(b)) {
        // Parse datetimes
        const datetimeA = parseDatetime(a);
        const datetimeB = parseDatetime(b);
        return datetimeA - datetimeB;
    }

    // Check if the values are sizes in KB, MB, GB, TB
    const sizeRegex = /^(\d+(\.\d+)?)\s?(KB|MB|GB|TB)$/i; // Accepts formats like '356 KB', '23.5 MB', '12 GB', '1728127 KB', '1 TB'
    if (sizeRegex.test(a) && sizeRegex.test(b)) {
        const [, sizeA, , unitA] = (a.match(sizeRegex) || []);
        const [, sizeB, , unitB] = (b.match(sizeRegex) || []);

        const sizeMap = { 'KB': 1, 'MB': 1024, 'GB': 1024 * 1024, 'TB': 1024 * 1024 * 1024 };

        const sizeValueA = parseFloat(sizeA) * (sizeMap[unitA.toUpperCase()] || 1);
        const sizeValueB = parseFloat(sizeB) * (sizeMap[unitB.toUpperCase()] || 1);

        return sizeValueA - sizeValueB;
    }

    // Default comparison (for strings)
    return a.localeCompare(b);
}

function parseDate(dateString) {
    const [day, month, year] = dateString.split(/[-\/]/).map(Number);
    return new Date(year, month - 1, day); // month is 0-based in JavaScript Date
}

function parseDatetime(datetimeString) {
    const [datePart, timePart] = datetimeString.split(' ');
    const [hours, minutes, seconds] = timePart.split(':').map(Number);
    const [day, month, year] = datePart.split(/[-\/]/).map(Number);
    return new Date(year, month - 1, day, hours, minutes, seconds);
}



function sortTable(n, id) {
    // algorithm for sorting disks and partitions as they are in same table rows
    if (id == "allPhysicalDrives-Table") {
        console.log(sortDirection)
        let partition = document.querySelectorAll("#sort-helper-partition");
        let main = document.querySelectorAll("#sort-helper-main");
        const partitionRows = document.querySelectorAll(".partition-row");
        partitionRows.forEach(function (row) {
            
            row.style.display = "none";
        });

        if (sortDirection == 'descending') {
            partition.forEach(function (row) {
                row.innerHTML = "0";
                row.style.display = "none";
            });
            main.forEach(function (row) {
                row.innerHTML = "1";
            });
        }
        else if (sortDirection == 'ascending') {
            partition.forEach(function (row) {
                row.innerHTML = "1";
                row.style.display = "none";
            });
            main.forEach(function (row) {
                row.innerHTML = "0";
            });
        }
        
    }

    // actual sorting algorithm
    let table = document.getElementById(id);
    let tbody = table.querySelector('tbody');
    let rows = Array.from(tbody.rows);

    rows = mergeSort(rows, (a, b) => {
        let x = a.getElementsByTagName('td')[n].textContent.trim();
        let y = b.getElementsByTagName('td')[n].textContent.trim();

        // Compare values based on the sorting direction
        if (sortDirection === 'ascending') {
            return compareValues(x, y);
        } else {
            return compareValues(y, x);
        }
    });

    let newTbody = document.createElement('tbody');
    rows.forEach(row => newTbody.appendChild(row));

    table.replaceChild(newTbody, tbody);

    sortDirection = (sortDirection === 'ascending') ? 'descending' : 'ascending';
}


// Sorting End


// Notifications start

let box = document.getElementById("notification-box");
let down = false;

function toggleNotification() {
    if (down) {
        box.style.height = "0px";
        box.style.opacity = 0;
        down = false;
    } else {
        box.style.height = "550px";
        box.style.opacity = 1;
        down = "true";
    }
}

function loadNotifications() {
    $.ajax({
        url: '/Notification/GetNotifications', // Update the URL to match your controller endpoint
        method: 'GET',
        success: function (notifications) {
            // Clear existing notifications
            $('#notification-container').empty();

            // Append new notifications
            notifications.forEach(function (notification) {
                var notificationContent = `
                    <div class="notification-content">
                        <h6 style="color:blue">${notification.assetID}</h6>
                        <h6>${notification.header}</h6>
                    </div>`;
                $('#notification-container').append(notificationContent);
            });

            // Update the notification count
            if (notifications == null) {
                var notificationCount = 0;
            } else {
                var notificationCount = notifications.length;
                
            }
            if (notificationCount > 90) {
                $('#notification-box h2 span').html('&infin;');
                $('.notification-button__badge').html('&infin;');
                $('.notification-page-title span').text(notificationCount);
            }
            else {
                $('#notification-box h2 span').text(notificationCount);
                $('.notification-button__badge').text(notificationCount);
                $('.notification-page-title span').text(notificationCount);
            }
           

            // Add click event listener to notification items
            $('.notification-content').click(function () {
                window.location.href = '/Notification/Index'; // Redirect to Notification Controller's Index action
            });
        },
        error: function (error) {
            console.error('Error fetching notifications:', error);
        }
    });
}

// Call loadNotifications function when the page loads or when needed
$(document).ready(function () {
    loadNotifications();

    // Call loadNotifications() every 10 seconds
    setInterval(loadNotifications, 5000000);
});


function markAllNotificationsAsRead() {
    $.ajax({
        url: '/Notification/MarkAllNotificationsAsRead', // Update the URL to match your controller endpoint
        method: 'GET',
        success: function () {
            notificationRows = document.getElementsByClassName("notification-page-body-row");
            Array.from(notificationRows).forEach(function (notification) {
                notification.style.backgroundColor = '#dddddd';
            });
            loadNotifications();
        },
        error: function (error) {
            console.error('Error marking notification as read:', error);
        }
    });
}

function showNotificationContent(clickedElement, notificationId) {
    // Get the parent notification body row
    var notificationBodyRow = clickedElement.closest('.notification-page-body-row');

    // Get the notification content within the clicked notification
    var notificationContent = notificationBodyRow.querySelector('.notification-page-content');
    var notificationHead = notificationBodyRow.querySelector('.notification-page-head');
    var viewButton = notificationBodyRow.querySelector('.view-btn');

    // Check if the clicked content is already expanded
    var isExpanded = notificationContent.classList.contains('show');

    // Reset all buttons to "View"
    var allViewButtons = document.querySelectorAll('.view-btn');
    allViewButtons.forEach(function (button) {
        button.innerText = "View";
    });

    // Hide all notification bodies and remove border-bottom from all notification heads
    var notificationBodies = document.querySelectorAll('.notification-page-content');
    var notificationHeads = document.querySelectorAll('.notification-page-head');

    notificationBodies.forEach(function (body) {
        body.classList.remove('show');
    });
    notificationHeads.forEach(function (head) {
        head.classList.remove('border-bottom');
    });

    // Toggle display of content
    if (!isExpanded) {
        // Expand the content
        notificationContent.classList.add('show');
        notificationHead.classList.add('border-bottom');
        clickedElement.innerText = "Hide";

        // Make AJAX call to mark notification as read
        markNotificationAsRead(notificationId);
    }
}

function markNotificationAsRead(notificationId) {
    $.ajax({
        url: '/Notification/IsReadNotification', // Update the URL to match your controller endpoint
        method: 'GET',
        data: { id: notificationId }, // Pass notification id as parameter
        success: function () {
            // If the request is successful, change the background color of the notification row
            var notificationRow = $('#notification-row-' + notificationId); // Assuming you have an ID for each notification row
            notificationRow.css('background', '#dddddd');
            loadNotifications();
        },
        error: function (error) {
            console.error('Error marking notification as read:', error);
        }
    });
}

function deleteNotification(notificationId) {
    $.ajax({
        url: '/Notification/DeleteNotification', // Update the URL to match your controller endpoint
        method: 'GET',
        data: { id: notificationId }, // Pass notification id as parameter
        success: function () {
            // If the request is successful, change the background color of the notification row
            var notificationRow = $('#notification-row-' + notificationId); // Assuming you have an ID for each notification row
            notificationRow.css('display', null);
            loadNotifications();
        },
        error: function (error) {
            console.error('Error marking notification as read:', error);
        }
    });
}




//Notifications End


//Loader


function redirectToDeviceInfo(assetId) {
    const loader = document.querySelector(".loader-hidden");
    loader.classList.add("loader"); // Show loader
    loader.classList.remove("loader-hidden"); // Show loader

    window.location.href = `/WorkStation/DeviceInfo/${assetId}`;
    
}

//Loader End

// Home page Pi Charts
$(document).ready(function () {    
    // Initialize the Chart.js pie chart
    //let isctx = document.getElementById("myChart");
    //if (isctx == null) {
    //    return;
    //}
    
    //// Initialize the Chart.js pie chart
    //var ctx = document.getElementById("myChart").getContext('2d');    
    //var myChart = new Chart(ctx, {
    //    type: 'pie',
    //    data: {
    //        labels: ["Approved", "Unapproved"], // Labels for the pie chart
    //        datasets: [{
    //            backgroundColor: [
    //                //"#2ecc71", // Color for Approved
    //                //"#e74c3c"  // Color for Unapproved
    //                "#b7a69e", // Color for Approved
    //                "#ff6f61"
    //            ],
    //            data: [0, 0] // Initial data (will be updated later)
    //        }]
    //    }
    //});
    
    // Fetch data from controller action using AJAX
    $.ajax({
        url: '/Home/GetApprovedVsUnapprovedDevices',
        type: 'GET',
        dataType: 'json',
        success: function (data) {            
            if (data !== null && data.length === 2) {
                $('#span-reported-devices').text(data[0] + data[1]);
                $('#span-approved-devices').text(data[0]);
                $('#span-unapproved-devices').text(data[1]);
            } else {
                $('#span-reported-device').text('0');
            }
        },
        error: function (xhr, status, error) {
            console.error('Error fetching data:', error);
        }
    });

});

//$(document).ready(function () {
//    // Initialize the Chart.js pie chart
//    let isctx = document.getElementById("myChart2");
//    if (isctx == null) {
//        return;
//    }

//    var ctx = document.getElementById("myChart2").getContext('2d');
//    var myChart = new Chart(ctx, {
//        type: 'pie',
//        data: {
//            labels: ["Yes", "No"], // Labels for the pie chart
//            datasets: [{
//                backgroundColor: [
//                    "#b7a69e", // Color for Approved
//                    "#ffb94e"  // Color for Unapproved
//                ],
//                data: [0, 0] // Initial data (will be updated later)
//            }]
//        }
//    });

//    // Fetch data from controller action using AJAX
//    $.ajax({
//        url: '/Home/GetInITAMvsOutITAM',
//        type: 'GET',
//        dataType: 'json',
//        success: function (data) {
//            if (data !== null && data.length === 2) {
//                // Update the data of the chart with fetched data
//                myChart.data.datasets[0].data = data;
//                // Update the chart
//                myChart.update();
//                document.getElementById("approvedDevices").innerHTML = data[0] + data[1];
//            } else {
//                document.getElementById("approvedDevices").innerHTML = 0;
//            }
//        },
//        error: function (xhr, status, error) {
//            console.error('Error fetching data:', error);
//        }
//    });

//});
// Pie chart end


// Polling Loging Status
function pollLoginStatus() {
    // Make a request to the GetLoginStatus endpoint
    $.ajax({
        url: '/Home/GetLoginStatus', // Adjust the URL to match your controller action route
        type: 'GET',
        dataType: 'json',
        success: function (isOnline) {
            console.log(isOnline);
            // Check if the response indicates that the user should be logged out
            if (isOnline == 'false') {
                
                // Redirect to the logout action
                window.location.href = '/Account/Logout'; // Adjust the URL as per your application's routes
            } else {
                // Poll the login status again after a certain interval
                setTimeout(pollLoginStatus, 5000); // Poll every 5 seconds (adjust as needed)
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            console.error('Error fetching login status:', errorThrown);
            // Retry polling after a certain interval in case of an error
            setTimeout(pollLoginStatus, 5000); // Retry every 5 seconds (adjust as needed)
        }
    });
}

// Start polling the login status
pollLoginStatus();

/*Table Search, Paging*/ /* Dev: Viraj; Date: 06-04-2024 */
$(document).ready(function () {    
    const customDiv = '<div class="my-custom-div">Custom Content</div>';
    $('#Device-Table').DataTable({
        searching: true,  // Enable searching/filtering
        paging: true,     // Enable pagination
        ordering: false,    // Disable sorting
        lengthMenu: [[10, 25, 50, 75, 100], [10, 25, 50, 75, 100]], // Customize the options

        
    });
});


// Dev: Viraj; Date:24-05-2024; Popup functionality
function showInPopup(url, title) {
    $.ajax({
        type: 'GET',
        url: url,
        success: function (res) {
            $('#form-modal .modal-body').html(res);
            $('#form-modal .modal-title').html(title);
            $('#form-modal').modal('show');
        }
    });
}

//function Details(id) {
//    debugger;
//    alert('clicked Details: ' + id);
//    $.ajax({
//        type: "GET",
//        url: "Workstation/AddInRegistry?ID=" + id,
//        success: function (res) {
//            alert('success');
//            console.log(res);
//            $("#form-modal .modal-body").html(res);
//            $("#form-modal").modal('show');
//        }
//    });
//}

$(document).ready(function () {
    $("#detailCard").click(function () {
        $.ajax({
            url: $(this).attr("formaction"),
        }).done(function (res) {
            $("#form-modal .modal-body").html(res);
            $("#form-modal").modal('show');
        });
    });
});




