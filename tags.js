$(document).ready(function () {
    loadBLData('@Url.Action("GetBLsoftwares", "YourControllerName")');
});

function toggleTagValue(tagId) {
    var tagValueElement = document.getElementById('tag-value-' + tagId);
    if (tagValueElement.style.display === 'block') {
        tagValueElement.style.display = 'none';
    } else {
        tagValueElement.style.display = 'block';
    }
}

function updateTag(tagId) {    
    var apiUrl = '/Tags/UpdateTag';
    console.log(tagId);
    $.ajax({
        url: apiUrl,
        type: 'GET', // or 'GET' depending on your API action
        dataType: "JSON",
        contentType: 'application/json; charset=utf-8',
        data: { tagId: tagId, status: false },
        success: function (response) {
            console.log(response);
            var tagValueElement = document.getElementById('tag-' + tagId);
            tagValueElement.style.display = 'none';
            location.reload();
        },
        error: function (error) {
            console.log('Error:', error);
            // Handle the error as needed
        }
    });   
}

function addTags() {
    // Use querySelector if you expect only one element
    var tagValueElement = document.querySelector('.add-tags');
    var addButton = document.querySelector('#add-button');

    if (tagValueElement.style.display === 'block') {
        tagValueElement.style.display = 'none';
        addButton.innerText = "Add"
    } else {
        tagValueElement.style.display = 'block';
        addButton.innerText = "Collapse"
    }
}

function addTag(tagId) {
    var apiUrl = '/Tags/UpdateTag';
    console.log(tagId);
    $.ajax({
        url: apiUrl,
        type: 'GET', // or 'GET' depending on your API action
        dataType: "JSON",
        contentType: 'application/json; charset=utf-8',
        data: { tagId: tagId, status: true },
        success: function (response) {
            console.log(response);
            location.reload();
        },
        error: function (error) {
            console.log('Error:', error);
            // Handle the error as needed
        }
    });
}


// Call the function to load data when the page is ready


// Function to load BL software data using Ajax
function loadBLData() {
    $.ajax({
        type: "GET",
        url: "/Tags/GetBLsoftwares",
        dataType: "JSON",
        contentType: 'application/json; charset=utf-8',
        success: function (result) {
            if (result.bLsoftwares) {
                // Clear existing rows
                $('#blTable tbody').empty();
                
                $.each(result.bLsoftwares, function (index, item) {
                    
                    $('#blTable tbody').append('<tr><td><b>' + item.assetId + '<b></td><td class="blvalue">' + item.name + '</td></tr>');
                });
                
            } else {
                // Handle error or display a message
                console.log("Error loading BL software data");
            }
        },
        error: function () {
            console.log("Error loading BL software data");
        }
    });
}