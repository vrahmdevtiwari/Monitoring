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
            console.log("Working");
            var notificationRow = $('#notification-row-' + notificationId); // Assuming you have an ID for each notification row
            notificationRow.css('display', 'none');
            loadNotifications();
        },
        error: function (error) {
            console.error('Error marking notification as read:', error);
        }
    });
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

