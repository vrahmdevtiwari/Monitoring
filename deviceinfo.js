
document.querySelectorAll(".details-table").forEach(function (element) {
    element.style.display = "none";
});

function showExtraDetails() {
    var button = document.querySelector("#seemoreButton");
    var extraDetailsRows = document.querySelectorAll(".extra-details");

    if (button.innerHTML === "See more...") {
        extraDetailsRows.forEach(function (row) {
            row.style.display = "table-row";
        });
        button.innerHTML = "See less...";
    } else if (button.innerHTML === "See less...") {
        extraDetailsRows.forEach(function (row) {
            row.style.display = "none";
        });
        button.innerHTML = "See more...";
    }
}

function displayTable(table) {
    var tableStatus = document.querySelector("#" + table + "-table").style.display;
    if (tableStatus == "none") {
        //Reset hiding all tables and show expand list icon
        document.querySelectorAll(".details-table").forEach(function (element) {
            element.style.display = "none";
        });
        document.querySelectorAll(".details-bar-minus").forEach(function (element) {
            element.style.display = "none";
        });
        document.querySelectorAll(".details-bar-plus").forEach(function (element) {
            element.style.display = "block";
        });

        // Set icon and display table for selected bar
        document.querySelector("#" + table + "-table").style.display = "block";
        document.querySelector("#" + table + "-bar-plus").style.display = "none";
        document.querySelector("#" + table + "-bar-minus").style.display = "block";
        document.querySelector(".activeNetworkDetails").style.display = "none";
    }
    else {
        document.querySelectorAll(".details-table").forEach(function (element) {
            element.style.display = "none";
        });
        document.querySelectorAll(".details-bar-minus").forEach(function (element) {
            element.style.display = "none";
        });
        document.querySelectorAll(".details-bar-plus").forEach(function (element) {
            element.style.display = "block";
        });
        var allDetails = document.querySelectorAll(".activeNetworkDetails");
        allDetails.forEach(function (detail) {
            detail.style.display = "none"
        });

    }


}

function toggleDetails(button) {
    

    var detailsDiv = button.parentElement.nextElementSibling;
    if (detailsDiv.style.display === "none" || detailsDiv.style.display === "") {
        var allDetails = document.querySelectorAll(".activeNetworkDetails");
        allDetails.forEach(function (detail) {
            detail.style.display = "none"
        });
        detailsDiv.style.display = "block";
    } else {
        detailsDiv.style.display = "none";
    }
    
}


    














