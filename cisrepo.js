document.addEventListener('DOMContentLoaded', () => {

    const toggle = document.getElementById('dropdownToggle');
    const options = document.getElementById('dropdownOptions');
    const tagsContainer = document.getElementById('selectedTags');

    // ─────────────────────────────────────────────────────────────
    // DROPDOWN TOGGLE — position: fixed use kar raha hai
    // isliye manually position calculate karni hogi
    // ─────────────────────────────────────────────────────────────
    toggle.addEventListener('click', (e) => {
        e.stopPropagation();
        const isOpen = options.classList.contains('open');
        options.classList.remove('open');
        if (!isOpen) {
            positionDropdown();
            options.classList.add('open');
        }
    });

    // Dropdown ko toggle ke neeche ya upar correctly position karo
    function positionDropdown() {
        const rect = toggle.getBoundingClientRect();
        const dropH = 200; // max-height of dropdown
        const spaceBelow = window.innerHeight - rect.bottom;
        const spaceAbove = rect.top;

        options.style.width = rect.width + 'px';
        options.style.left = rect.left + 'px';

        if (spaceBelow >= dropH || spaceBelow >= spaceAbove) {
            // Neeche open karo
            options.style.top = (rect.bottom + window.scrollY) + 'px';
            options.style.bottom = 'auto';
        } else {
            // Upar open karo
            options.style.top = 'auto';
            options.style.bottom = (window.innerHeight - rect.top + window.scrollY) + 'px';
        }
    }

    // Scroll ya resize pe position update karo
    window.addEventListener('scroll', () => {
        if (options.classList.contains('open')) positionDropdown();
    }, true);
    window.addEventListener('resize', () => {
        if (options.classList.contains('open')) positionDropdown();
    });

    document.addEventListener('click', e => {
        if (!toggle.contains(e.target) && !options.contains(e.target)) {
            options.classList.remove('open');
        }
    });

    // ─────────────────────────────────────────────────────────────
    // CHECKBOX CHANGE → TAGS
    // ─────────────────────────────────────────────────────────────
    options.addEventListener('change', e => {
        if (e.target.classList.contains('product-family-checkbox')) {
            e.target.checked
                ? addTag(e.target.value, e.target.dataset.label)
                : removeTag(e.target.value);
        }
    });

    tagsContainer.addEventListener('click', e => {
        if (e.target.classList.contains('tag-close')) {
            const val = e.target.dataset.value;
            removeTag(val);
            const cb = document.getElementById(`pf_${val}`);
            if (cb) cb.checked = false;
            loadPatches();
        }
    });

    function addTag(value, label) {
        if (document.querySelector(`.tag[data-value="${value}"]`)) return;
        const tag = document.createElement('span');
        tag.className = 'tag';
        tag.dataset.value = value;
        tag.innerHTML = `${label} <span class="tag-close" data-value="${value}">&times;</span>`;
        tagsContainer.appendChild(tag);
    }

    function removeTag(value) {
        const tag = document.querySelector(`.tag[data-value="${value}"]`);
        if (tag) tag.remove();
    }

    document.querySelectorAll('.product-family-checkbox:checked')
        .forEach(cb => addTag(cb.value, cb.dataset.label));

    // ─────────────────────────────────────────────────────────────
    // PAGINATION SETUP
    // ─────────────────────────────────────────────────────────────
    let _allOsPatches = [];
    let _allSoftwarePatches = [];

    Pagination.init({
        key: 'osPatches',
        loader: (page, pageSize) => loadPatchesPage('osPatches', page, pageSize),
        pageSize: 25,
        scrollSelector: '#osPatchContainer',
        active: true
    });

    Pagination.init({
        key: 'softwarePatches',
        loader: (page, pageSize) => loadPatchesPage('softwarePatches', page, pageSize),
        pageSize: 25,
        scrollSelector: '#softwarePatchContainer',
        active: false
    });

    document.getElementById('os-tab').addEventListener('click', () => {
        Pagination.activate('osPatches');
    });
    document.getElementById('software-tab').addEventListener('click', () => {
        Pagination.activate('softwarePatches');
    });

    function loadPatchesPage(key, page, pageSize) {
        const patches = key === 'osPatches' ? _allOsPatches : _allSoftwarePatches;
        const total = patches.length;
        const start = (page - 1) * pageSize;
        const slice = patches.slice(start, start + pageSize);

        if (key === 'osPatches') {
            renderTable('osPatchContainer', slice, [
                { key: 'updateId', label: 'Update ID' },
                { key: 'updateOS', label: 'Update OS' },
                { key: 'bitRate', label: 'Bit Rate' },
                { key: 'title', label: 'Title' },
                { key: 'product', label: 'Product' },
                { key: 'kbNumber', label: 'KB Number' },
                { key: 'classification', label: 'Classification' },
                { key: 'version', label: 'Version' }
            ]);
        } else {
            renderTable('softwarePatchContainer', slice, [
                { key: 'updateId', label: 'Update ID' },
                { key: 'softwareName', label: 'Software Name' },
                { key: 'vendor', label: 'Vendor' },
                { key: 'classification', label: 'Classification' },
                { key: 'version', label: 'Version' },
                { key: 'bitRate', label: 'Bit Rate' },
                { key: 'platform', label: 'Platform' }
            ]);
        }

        Pagination.update(key, total, page, pageSize);
    }

    // ─────────────────────────────────────────────────────────────
    // SAVE BUTTON
    // ─────────────────────────────────────────────────────────────
    document.getElementById('saveOptionsBtn').addEventListener('click', async function () {
        const selected = Array.from(document.querySelectorAll('.product-family-checkbox:checked'))
            .map(cb => cb.value);
        if (!selected.length) return alert('Please select at least one option.');

        setLoading(this, true);
        loadingStart();
        try {
            const res = await fetch('/CISRepo/SaveUserFamilies', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ Type: this.dataset.type, Families: selected })
            });
            const data = await res.json();
            showToast(data.message || 'Saved successfully', data.success ? 'success' : 'warning');
            renderPatches(data);
        } catch {
            showToast('Failed to save Product Family', 'danger');
        } finally {
            loadingStop();
            setLoading(this, false);
        }
    });

    // ─────────────────────────────────────────────────────────────
    // SYNC BUTTON
    // ─────────────────────────────────────────────────────────────

    document.getElementById('syncNowBtn').addEventListener('click', async function () {
        const selected = Array.from(document.querySelectorAll('.product-family-checkbox:checked'))
            .map(cb => cb.value);
        if (!selected.length) return alert('Select at least one family to sync.');

        setLoading(this, true);
        loadingStart();

        try {
            const res = await fetch('/CISRepo/SyncNow', { method: 'GET' });

            // Step 1: Pehle HTTP status check karo
            // res.json() se pehle — taaki parse fail hone ki situation na aaye
            if (!res.ok) {
                // Server ne 500 diya — try karo parse karne ka
                // Agar parse bhi fail ho to catch handle karega
                const errData = await res.json().catch(() => null);
                showToast(errData?.message || 'Failed to sync. Please try again.', 'danger');
                return;
            }

            // Step 2: Ab safely parse karo — kyunki res.ok hai (200-299)
            const data = await res.json();

            // Step 3: Business logic
            if (data.noFamilies) {
                showToast(data.message, 'warning');
            } else if (data.alreadySynced) {
                showToast(data.message, 'info');
            } else {
                showToast(data.message, 'success');
                renderPatches(data);
            }

        } catch {
            // Sirf in cases mein aayega:
            // 1. Network failure (internet nahi)
            // 2. Server respond hi nahi kiya
            showToast('Failed to sync. Please check your connection.', 'danger');
        } finally {
            loadingStop();
            setLoading(this, false);
        }
    });
    //document.getElementById('syncNowBtn').addEventListener('click', async function () {
    //    const selected = Array.from(document.querySelectorAll('.product-family-checkbox:checked'))
    //        .map(cb => cb.value);
    //    if (!selected.length) return alert('Select at least one family to sync.');

    //    setLoading(this, true);
    //    loadingStart();
    //    try {
    //        const res = await fetch('/CISRepo/SyncNow', {
    //            method: 'GET'
    //        });
    //        const data = await res.json();
    //        showToast(data.message || 'Sync completed', 'success');
    //        renderPatches(data);
    //    } catch {
    //        showToast('Failed to sync', 'danger');
    //    } finally {
    //        loadingStop();
    //        setLoading(this, false);
    //    }
    //});

    // ─────────────────────────────────────────────────────────────
    // LOAD PATCHES
    // ─────────────────────────────────────────────────────────────
    async function loadPatches() {
        loadingStart();
        try {
            const res = await fetch('/CISRepo/GetOSPatchAndSoftwares', { method: 'GET' });
            const data = await res.json();
            renderPatches(data);
        } catch (err) {
            console.error(err);
        } finally {
            loadingStop();
        }
    }

    // ─────────────────────────────────────────────────────────────
    // RENDER TABLE
    // ─────────────────────────────────────────────────────────────
    function renderTable(containerId, patches, columns) {
        const container = document.getElementById(containerId);

        if (!patches || patches.length === 0) {
            container.innerHTML = '<div class="table-container"><table><tbody><tr><td>No data available</td></tr></tbody></table></div>';
            return;
        }

        let html = "<div class='table-container'><table class='table table-bordered table-hover custom-table'><thead><tr>";
        columns.forEach(col => {
            html += `<th>${col.label}</th>`;
        });
        html += "</tr></thead><tbody>";

        patches.forEach(patch => {
            html += "<tr>";
            columns.forEach(col => {
                const val = patch[col.key] ?? "";
                // Escape HTML entities for title attribute
                const escaped = String(val).replace(/"/g, '&quot;').replace(/</g, '&lt;');
                html += `<td title="${escaped}">${val}</td>`;
            });
            html += "</tr>";
        });

        html += "</tbody></table></div>";
        container.innerHTML = html;
    }

    // ─────────────────────────────────────────────────────────────
    // RENDER PATCHES — cache + page 1 se start
    // ─────────────────────────────────────────────────────────────
    function renderPatches(data) {
        _allOsPatches = data.osPatches || [];
        _allSoftwarePatches = data.softwarePatches || [];

        loadPatchesPage('osPatches', 1, Pagination.getState('osPatches').pageSize);
        loadPatchesPage('softwarePatches', 1, Pagination.getState('softwarePatches').pageSize);

        Pagination.activate('osPatches');
    }

    // ─────────────────────────────────────────────────────────────
    // TOAST
    // ─────────────────────────────────────────────────────────────
    function showToast(msg, type = 'success') {
        const toastEl = document.getElementById('successToast');
        const toastBody = document.getElementById('toastMessage');
        toastBody.textContent = msg;
        toastEl.className = `toast align-items-center text-white bg-${type} border-0`;
        new bootstrap.Toast(toastEl, { autohide: true, delay: 3000 }).show();
    }

    // ─────────────────────────────────────────────────────────────
    // LOADING STATE
    // ─────────────────────────────────────────────────────────────
    function setLoading(button, isLoading) {
        if (isLoading) {
            button.classList.add('loading');
            button.disabled = true;
        } else {
            button.classList.remove('loading');
            button.disabled = false;
        }
    }

    // ─────────────────────────────────────────────────────────────
    // INITIAL LOAD
    // ─────────────────────────────────────────────────────────────
    loadPatches();
});




//document.addEventListener('DOMContentLoaded', () => {
//    const toggle = document.getElementById('dropdownToggle');
//    const options = document.getElementById('dropdownOptions');
//    const tagsContainer = document.getElementById('selectedTags');

//    // Toggle dropdown
//    toggle.addEventListener('click', () => options.classList.toggle('open'));
//    document.addEventListener('click', e => {
//        if (!toggle.contains(e.target) && !options.contains(e.target)) {
//            options.classList.remove('open');
//        }
//    });

//    // Checkbox change → update tags
//    options.addEventListener('change', e => {
//        if (e.target.classList.contains('product-family-checkbox')) {
//            e.target.checked ? addTag(e.target.value, e.target.dataset.label)
//                : removeTag(e.target.value);
//        }
//    });

//    // Remove tag → uncheck checkbox
//    tagsContainer.addEventListener('click', e => {
//        if (e.target.classList.contains('tag-close')) {
//            const val = e.target.dataset.value;
//            removeTag(val);
//            const cb = document.getElementById(`pf_${val}`);
//            if (cb) cb.checked = false;
//            loadPatches();
//        }
//    });

//    // Add tag element
//    function addTag(value, label) {
//        if (document.querySelector(`.tag[data-value="${value}"]`)) return;
//        const tag = document.createElement('span');
//        tag.className = 'tag';
//        tag.dataset.value = value;
//        tag.innerHTML = `${label} <span class="tag-close" data-value="${value}">&times;</span>`;
//        tagsContainer.appendChild(tag);
//    }

//    // Remove tag element
//    function removeTag(value) {
//        const tag = document.querySelector(`.tag[data-value="${value}"]`);
//        if (tag) tag.remove();
//    }

//    // Initialize tags for pre-checked checkboxes
//    document.querySelectorAll('.product-family-checkbox:checked')
//        .forEach(cb => addTag(cb.value, cb.dataset.label));

//    // Save product families
//    document.getElementById('saveOptionsBtn').addEventListener('click', async function () {
//        const selected = Array.from(document.querySelectorAll('.product-family-checkbox:checked'))
//            .map(cb => cb.value);
//        if (!selected.length) return alert('Please select at least one option.');

//        loadingStart();
//        try {
//            const res = await fetch('/CISRepo/SaveUserFamilies', {
//                method: 'POST',
//                headers: { 'Content-Type': 'application/json' },
//                body: JSON.stringify({ Type: this.dataset.type, Families: selected })
//            });
//            const data = await res.json();
//            showToast(data.message || 'Saved successfully', data.success ? 'success' : 'warning');
//            loadPatches();
//        } catch {
//            showToast('Failed to save Product Family', 'danger');
//        } finally {
//            loadingStop();
//        }
//    });

//    // Sync Now
//    document.getElementById('syncNowBtn').addEventListener('click', async () => {
//        const selected = Array.from(document.querySelectorAll('.product-family-checkbox:checked'))
//            .map(cb => cb.value);
//        if (!selected.length) return alert('Select at least one family to sync.');
//        loadingStart();
//        try {
//            const res = await fetch('/CISRepo/SyncNow', {
//                method: 'POST',
//                headers: { 'Content-Type': 'application/json' },
//                body: JSON.stringify(selected)
//            });
//            const data = await res.json();
//            showToast(data.message || 'Sync completed', 'success');
//            renderPatches(data);
//        } catch {
//            showToast('Failed to sync', 'danger');
//        } finally {
//            loadingStop();
//        }
//    });

//    // Load patches
//    async function loadPatches() {
//        loadingStart();
//        try {
//            const res = await fetch('/CISRepo/GetOSPatchAndSoftwares', {
//                method: 'GET',
//            });
//            const data = await res.json();
//            renderPatches(data);
//        } catch (err) {
//            console.error(err);
//        } finally {
//            loadingStop();
//        }
//    }


//    // Render OS & Software patches
//    function renderTable(containerId, patches, columns) {

//        const container = document.getElementById(containerId);

//        if (!patches || patches.length === 0) {
//            container.innerHTML = "No data available";
//            return;
//        }

//        let html = "<table class='table table-bordered table-hover custom-table'><thead><tr>";

//        // Headers (using label)
//        columns.forEach(col => {
//            html += `<th>${col.label}</th>`;
//        });

//        html += "</tr></thead><tbody>";

//        // Rows (using key)
//        patches.forEach(patch => {
//            html += "<tr>";

//            columns.forEach(col => {
//                html += `<td>${patch[col.key] ?? ""}</td>`;
//            });

//            html += "</tr>";
//        });

//        html += "</tbody></table>";

//        container.innerHTML = html;
//    }
//    function renderPatches(data) {

//        renderTable("osPatchContainer", data.osPatches, [
//            { key: "updateId", label: "Update ID" },
//            { key: "updateOS", label: "Update OS" },
//            { key: "bitRate", label: "Bit Rate" },
//            { key: "title", label: "Title" },
//            { key: "product", label: "Product" },
//            { key: "kbNumber", label: "KB Number" },
//            { key: "classification", label: "Classification" },
//            { key: "version", label: "Version" }
//        ]);
//        renderTable("softwarePatchContainer", data.softwarePatches, [
//            { key: "updateId", label: "Update ID" },
//            { key: "softwareName", label: "Software Name" },
//            { key: "vendor", label: "Vendor" },
//            { key: "classification", label: "Classification" },
//            { key: "version", label: "Version" },
//            { key: "bitRate", label: "Bit Rate" },
//            { key: "platform", label: "Platform" }
//        ]);
//    }
//    // Toast helper
//    function showToast(msg, type = 'success') {
//        const toastEl = document.getElementById('successToast');
//        const toastBody = document.getElementById('toastMessage');
//        toastBody.textContent = msg;

//        toastEl.className = `toast align-items-center text-white bg-${type} border-0`;
//        new bootstrap.Toast(toastEl, { autohide: true, delay: 3000 }).show();
//    }

//    // Initial load
//    loadPatches();
//    function setLoading(button, isLoading) {
//        if (isLoading) {
//            button.classList.add("loading");
//            button.disabled = true;
//        } else {
//            button.classList.remove("loading");
//            button.disabled = false;
//        }
//    }

//    document.getElementById("saveOptionsBtn").addEventListener("click", function () {
//        const btn = this;
//        setLoading(btn, true);

//        // Simulate API call
//        setTimeout(() => {
//            setLoading(btn, false);
//        }, 2000);
//    });

//    document.getElementById("syncNowBtn").addEventListener("click", function () {
//        const btn = this;
//        setLoading(btn, true);

//        // Simulate API call
//        setTimeout(() => {
//            setLoading(btn, false);
//        }, 2500);
//    });
//});