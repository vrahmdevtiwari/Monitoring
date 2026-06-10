/// <reference path="../../lib/jquery/dist/jquery.js" />

$(document).ready(function () {
    // Manage Profile
    //document.getElementById('ManageProfile').addEventListener('click', function () {
    $("#ManageProfile").on('click', function () {        
        // $('#profileModalBody').attr('width', '350px');
        // data-bs-toggle="modal" data-bs-target="#ManageProfileModel"
        $(this).attr('data-bs-toggle', 'modal').attr('data-bs-target', '#ManageProfileModel');
        
        $.ajax({
            url: 'Account/ManageProfile', // Replace with your actual controller name
            type: 'GET',
            beforeSend: function () {                
                loadingStart(); // Call before sending the request
            },
            success: function (data) {                
                $('#profileModalBody').html(data); // Load the partial view into the modal
                $("#ManageProfileModel").modal('show');
                // Wait for the DOM to finish rendering
                requestAnimationFrame(() => {
                    requestAnimationFrame(() => {
                        loadingStop();
                    });
                });
            },
            error: function (xhr, status, error) {                
                loadingStop();
                console.error("Error loading profile data:", error);
                $('#profileModalBody').html("<p>please wait, loading...</p>"); // Display error message
                $("#ManageProfileModel").modal('show');
            },
            complete: function () {
                loadingStop(); // Always called after success or error
            }
        });
    });

    // Registration
    $("#Register").on('click', function () {
        // data-bs-toggle="modal" data-bs-target="#ManageProfileModel"
        $(this).attr('data-bs-toggle', 'modal').attr('data-bs-target', '#ManageProfileModel');
        $.ajax({
            url: 'Account/Register', // Replace with your actual controller name
            type: 'GET',
            beforeSend: function () {
                loadingStart(); // Call before sending the request
            },
            success: function (data) {                
                $('#profileModalBody').html(data); // Load the partial view into the modal
                $("#ManageProfileModel").modal('show');
                loadingStop();
            },
            error: function (xhr, status, error) {
                loadingStop();
                console.error("Error loading profile data:", error);
                $('#profileModalBody').html("<p>please wait, loading...</p>"); // Display error message
                $("#ManageProfileModel").modal('show');
            },
            complete: function () {
                loadingStop(); // Always called after success or error
            }
        });
    });

    // Change Password
    $("#ChangePassword").on('click', function () {
        // data-bs-toggle="modal" data-bs-target="#ManageProfileModel"
        $(this).attr('data-bs-toggle', 'modal').attr('data-bs-target', '#ManageProfileModel');
        $.ajax({
            url: 'Account/ChangePassword', // Replace with your actual controller name
            type: 'GET',
            beforeSend: function () {
                loadingStart(); // Call before sending the request
            },
            success: function (data) {
                $('#profileModalBody').html(data); // Load the partial view into the modal
                $("#ManageProfileModel").modal('show');
                loadingStop();
            },
            error: function (xhr, status, error) {
                loadingStop();
                console.error("Error loading profile data:", error);
                $('#profileModalBody').html("<p>Error loading data.</p>"); // Display error message
                $("#ManageProfileModel").modal('show');
            },
            complete: function () {
                loadingStop(); // Always called after success or error
            }
        });
    });
})