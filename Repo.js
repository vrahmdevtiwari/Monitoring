document.addEventListener("DOMContentLoaded", function () {

    const selectAll = document.getElementById("selectAllFamilies");
    const checkboxes = document.querySelectorAll(".product-family-checkbox");
    const container = document.getElementById("familyContainer");
    const toggleBtn = document.getElementById("toggleFamiliesBtn");

    if (checkboxes.length === 0) return;

    // =============================
    // LOAD SAVED STATE
    // =============================
    const savedFamilies =
        JSON.parse(localStorage.getItem("selectedFamilies") || "[]");

    const savedActiveTab =
        localStorage.getItem("activeTab");

    // Restore checkbox selections
    checkboxes.forEach(cb => {
        if (savedFamilies.includes(cb.value)) {
            cb.checked = true;
        }

        cb.addEventListener("change", onSelectionChange);
    });

    // Restore Select All state
    updateSelectAllState();

    // Restore active tab
    if (savedActiveTab) {
        const tabBtn = document.querySelector(
            `[data-bs-target="${savedActiveTab}"]`
        );
        if (tabBtn) {
            new bootstrap.Tab(tabBtn).show();
        }
    }

    // Reload patches on refresh
    if (savedFamilies.length > 0) {
        loadPatches(savedFamilies);
    }

    // Save active tab when changed
    document.querySelectorAll('#patchTabs button')
        .forEach(tab => {
            tab.addEventListener('shown.bs.tab', function (event) {
                localStorage.setItem(
                    "activeTab",
                    event.target.getAttribute("data-bs-target")
                );
            });
        });

    // =============================
    // SELECT ALL
    // =============================
    if (selectAll) {
        selectAll.addEventListener("change", function () {
            checkboxes.forEach(cb => {
                cb.checked = selectAll.checked;
            });
            onSelectionChange();
        });
    }

    // =============================
    // MORE / LESS
    // =============================
    if (container && toggleBtn) {

        container.classList.add("collapsed");

        toggleBtn.addEventListener("click", function () {

            container.classList.toggle("expanded");
            container.classList.toggle("collapsed");

            toggleBtn.innerText =
                container.classList.contains("expanded")
                    ? "Less"
                    : "More...";
        });
    }

    // =============================
    // MAIN SELECTION HANDLER
    // =============================
    function onSelectionChange() {

        const selected = [...document.querySelectorAll(".product-family-checkbox")]
            .filter(cb => cb.checked)
            .map(cb => cb.value);

        updateSelectAllState();
        loadPatches();

        // 🔥 Send to backend (MongoDB)
        fetch('/CISRepo/SaveUserFamilies', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(selected)
        })
            .catch(err => console.error("Save failed:", err));
    }

    function updateSelectAllState() {

        if (!selectAll) return;

        const total = checkboxes.length;
        const checked =
            [...checkboxes].filter(cb => cb.checked).length;

        if (checked === total && total > 0) {
            selectAll.checked = true;
            selectAll.indeterminate = false;
        }
        else if (checked === 0) {
            selectAll.checked = false;
            selectAll.indeterminate = false;
        }
        else {
            selectAll.indeterminate = true;
        }
    }

    
});