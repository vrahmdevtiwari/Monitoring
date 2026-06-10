function approve(assetId) {
    var apiUrl = '/Workstation/ApproveOrDeny';
    console.log(assetId);
    $.ajax({
        url: apiUrl,
        type: 'GET', // or 'GET' depending on your API action
        dataType: "JSON",
        contentType: 'application/json; charset=utf-8',
        data: { assetId: assetId, approve: "approve" },
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

function deny(assetId) {
    var apiUrl = '/Workstation/ApproveOrDeny';
    console.log(assetId);
    $.ajax({
        url: apiUrl,
        type: 'GET', // or 'GET' depending on your API action
        dataType: "JSON",
        contentType: 'application/json; charset=utf-8',
        data: { assetId: assetId, approve: "delete" },
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


