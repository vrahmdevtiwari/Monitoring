/// <reference path="../../lib/jquery/dist/jquery.min.js" />

/**
 * ============================================================
 *  pagination.js  –  Reusable server-side pagination module
 *  Usage: include this file on any page that needs pagination.
 *
 *  Call: Pagination.init(config) to set up pagination for a tab/table.
 *  See bottom of file for full usage example.
 * ============================================================
 */

const Pagination = (function () {

    // ── Internal registry: one state object per "tab key" ──────────────────
    const _states = {};

    // ── Build the fixed bottom bar (only once per page) ────────────────────
    function _buildBar() {
        if (document.getElementById('pg-bar')) return;

        const bar = document.createElement('div');
        bar.id = 'pg-bar';
        bar.innerHTML = `
            <div id="pg-left">
                <label for="pg-size-select" style="margin:0;font-size:13px;white-space:nowrap;">Rows per page:</label>
                <select id="pg-size-select" class="form-select form-select-sm" style="width:auto;">
                    <option value="25" selected>25</option>
                    <option value="50">50</option>
                    <option value="75">75</option>
                    <option value="100">100</option>
                </select>
            </div>
            <div id="pg-buttons"></div>
            <div id="pg-info"></div>
        `;
        document.body.appendChild(bar);

        // Inject CSS once
        if (!document.getElementById('pg-style')) {
            const style = document.createElement('style');
            style.id = 'pg-style';
            style.textContent = `
    /* ─────────────────────────────────────────────
       Responsive Pagination Bar
    ───────────────────────────────────────────── */
    #pg-bar {
    position: fixed;
    bottom: 50px; /* height above footer */

    left: 0;
    right: 0;

    z-index: 1050;

    display: flex;
    align-items: center;
    justify-content: space-between;
    flex-wrap: wrap;

    gap: 10px;
    padding: 8px 16px;

    background: #fff;
    border-top: 1px solid #dee2e6;
    box-shadow: 0 -2px 6px rgba(0,0,0,0.08);

    width: 100%;
    box-sizing: border-box;
}

    #pg-left {
        display: flex;
        align-items: center;
        gap: 8px;
        flex-wrap: wrap;
    }

    #pg-buttons {
        display: flex;
        gap: 4px;
        align-items: center;
        justify-content: center;
        flex-wrap: wrap;
        flex: 1;
    }

    #pg-info {
        font-size: 13px;
        color: #555;
        white-space: nowrap;
    }

    

    /* Scrollable table wrapper */
    .pg-scroll-table {
    overflow-y: auto;
    overflow-x: auto;
    box-sizing: border-box;

    /* prevents scrollbar from touching pagination */
    padding-bottom: 10px;
}

    /* ─────────────────────────────────────────────
       Tablet
    ───────────────────────────────────────────── */
    @media (max-width: 992px) {
        #pg-bar {
            padding: 10px;
        }

        #pg-buttons button {
            min-width: 32px !important;
            padding: 4px 8px;
        }
    }

    /* ─────────────────────────────────────────────
       Mobile
    ───────────────────────────────────────────── */
    @media (max-width: 768px) {

        #pg-bar {
            flex-direction: column;
            align-items: stretch;
            text-align: center;
        }

        #pg-left {
            justify-content: center;
        }

        #pg-buttons {
            justify-content: center;
            width: 100%;
        }

        #pg-info {
            text-align: center;
            white-space: normal;
        }

        #pg-size-select {
            width: 100% !important;
        }

        body {
            padding-bottom: 120px;
        }
    }

    /* ─────────────────────────────────────────────
       Small Mobile
    ───────────────────────────────────────────── */
    @media (max-width: 480px) {

        #pg-buttons button {
            min-width: 28px !important;
            font-size: 12px;
            padding: 3px 6px;
        }

        #pg-info {
            font-size: 12px;
        }
    }
`; document.head.appendChild(style);
        }

        // Page-size change
        document.getElementById('pg-size-select').addEventListener('change', function () {
            const key = _getActiveKey();
            if (!key) return;
            const s = _states[key];
            s.pageSize = parseInt(this.value);
            s.page = 1;
            s.loader(s.page, s.pageSize);
        });

        // Recalculate table scroll height on resize
        window.addEventListener('resize', _applyTableScroll);
    }

    //// ── Work out which table wrapper should scroll ─────────────────────────
    function _applyTableScroll() {
        const key = _getActiveKey();
        if (!key) return;

        const s = _states[key];
        if (!s || !s.scrollSelector) return;

        const el = document.querySelector(s.scrollSelector);
        if (!el) return;

        const pgBar = document.getElementById('pg-bar');

        // Pagination bar height
        const barH = pgBar ? pgBar.offsetHeight : 60;

        // Footer height
        const footer = document.querySelector('.footer');
        const footerH = footer ? footer.offsetHeight : 50;

        // Table top position
        const rect = el.getBoundingClientRect();

        // Extra spacing so scrollbar stops above pagination
        const safeGap = 16;

        // Final available height
        const avail =
            window.innerHeight
            - rect.top
            - barH
            - footerH
            - safeGap;

        el.style.maxHeight = Math.max(150, avail) + 'px';

        el.classList.add('pg-scroll-table');
    }

    //function _applyTableScroll() {
    //    const key = _getActiveKey();
    //    if (!key) return;
    //    const s = _states[key];
    //    if (!s || !s.scrollSelector) return;

    //    const el = document.querySelector(s.scrollSelector);
    //    if (!el) return;

    //    // Available height = viewport − bar height − top of element − small buffer
    //    const barH = document.getElementById('pg-bar').offsetHeight || 52;
    //    const rect = el.getBoundingClientRect();
    //    const avail = window.innerHeight - rect.top - barH - 8;
    //    el.style.maxHeight = Math.max(150, avail) + 'px';
    //    el.classList.add('pg-scroll-table');
    //}

    // ── Find the currently active tab key ─────────────────────────────────
    function _getActiveKey() {
        for (const key in _states) {
            if (_states[key].active) return key;
        }
        return null;
    }

    // ── Render page buttons ────────────────────────────────────────────────
    function _render(key) {
        const s = _states[key];
        const { page, pageSize, total } = s;

        const totalPages = Math.max(1, Math.ceil(total / pageSize));
        const from = total === 0 ? 0 : (page - 1) * pageSize + 1;
        const to = Math.min(page * pageSize, total);

        document.getElementById('pg-info').textContent =
            `${from} – ${to} of ${total} records`;

        // Sync the page-size dropdown to this tab's setting
        const sel = document.getElementById('pg-size-select');
        if (sel && parseInt(sel.value) !== pageSize) sel.value = pageSize;

        const $btns = $('#pg-buttons').empty();

        const btn = (label, pg, disabled, active) => {
            const b = $(`<button class="btn btn-sm ${active ? 'btn-primary' : 'btn-outline-secondary'}"
                            style="min-width:34px;" ${disabled ? 'disabled' : ''}>${label}</button>`);
            if (!disabled && !active) {
                b.on('click', () => {
                    s.page = pg;
                    s.loader(s.page, s.pageSize);
                });
            }
            return b;
        };

        $btns.append(btn('«', 1, page === 1, false));
        $btns.append(btn('‹', page - 1, page === 1, false));

        let start = Math.max(1, page - 2);
        let end = Math.min(totalPages, start + 4);
        if (end - start < 4) start = Math.max(1, end - 4);

        if (start > 1) {
            $btns.append(btn('1', 1, false, false));
            if (start > 2) $btns.append($('<span style="align-self:center;padding:0 4px;">…</span>'));
        }
        for (let p = start; p <= end; p++) {
            $btns.append(btn(p, p, false, p === page));
        }
        if (end < totalPages) {
            if (end < totalPages - 1) $btns.append($('<span style="align-self:center;padding:0 4px;">…</span>'));
            $btns.append(btn(totalPages, totalPages, false, false));
        }

        $btns.append(btn('›', page + 1, page === totalPages, false));
        $btns.append(btn('»', totalPages, page === totalPages, false));

        // Re-apply scroll height whenever we re-render
        setTimeout(_applyTableScroll, 50);
    }

    // ══════════════════════════════════════════════════════════════════════
    //  PUBLIC API
    // ══════════════════════════════════════════════════════════════════════

    /**
     * Pagination.init(config)
     *
     * config = {
     *   key           : string   – unique key for this tab/table, e.g. 'approved'
     *   loader        : function(page, pageSize) – your fetch function
     *   pageSize      : number   – initial page size (default 25)
     *   scrollSelector: string   – CSS selector of the scrollable table wrapper
     *                              e.g. '.div-approved-table'
     *   active        : bool     – set true for the default visible tab
     * }
     */
    function init(config) {
        _buildBar();

        _states[config.key] = {
            key: config.key,
            loader: config.loader,
            page: 1,
            pageSize: config.pageSize || 25,
            total: 0,
            scrollSelector: config.scrollSelector || null,
            active: config.active || false
        };

        if (config.active) {
            _applyTableScroll();
        }
    }

    /**
     * Pagination.activate(key)
     * Call this when the user switches to a tab.
     * Syncs the bar to that tab's state.
     */
    function activate(key) {
        for (const k in _states) _states[k].active = false;
        if (_states[key]) {
            _states[key].active = true;
            _render(key);
            _applyTableScroll();
        }
    }

    /**
     * Pagination.update(key, total, page, pageSize)
     * Call this inside your loader's .then() after you get data back.
     * Updates the bar to reflect the new state.
     */
    function update(key, total, page, pageSize) {
        if (!_states[key]) return;
        _states[key].total = total;
        _states[key].page = page;
        _states[key].pageSize = pageSize;
        if (_states[key].active) _render(key);
    }

    /**
     * Pagination.getState(key)
     * Returns { page, pageSize } for the given key.
     * Useful for approve/delete callbacks that need to reload the same page.
     */
    function getState(key) {
        if (!_states[key]) return { page: 1, pageSize: 25 };
        return { page: _states[key].page, pageSize: _states[key].pageSize };
    }
  
    function show() {
        const bar = document.getElementById('pg-bar');
        if (bar) bar.style.display = 'flex';
    }

    function hide() {
        const bar = document.getElementById('pg-bar');
        if (bar) bar.style.display = 'none';
    }

    return { init, activate, update, getState, show, hide };  // ← update this line too

})();


/* ══════════════════════════════════════════════════════════════════════════════
   USAGE EXAMPLE  (put this in your page-specific JS, e.g. home-page.js)
   ══════════════════════════════════════════════════════════════════════════════

$(document).ready(function () {

    // 1. Register each tab with the pagination module
    Pagination.init({
        key            : 'approved',
        loader         : loadApproved,          // your fetch function
        pageSize       : 25,
        scrollSelector : '.div-approved-table', // the element that should scroll
        active         : true                   // this tab is shown first
    });

    Pagination.init({
        key            : 'unapproved',
        loader         : loadUnapproved,
        pageSize       : 25,
        scrollSelector : '.div-unapproved-table'
    });

    // 2. Load the default tab
    loadApproved(1, 25);

    // 3. Tab click — just call Pagination.activate(key) + your loader
    $(document).on('click', '.device-model-nav .nav-link', function () {
        $(this).closest('ul').find('.nav-link').removeClass('active');
        $(this).addClass('active');

        const type  = $(this).data('type');
        const tabId = $(this).data('tabid');

        if (tabId) { $('.tab-container').hide(); $('#' + tabId).show(); }

        if (type === 'approved') {
            Pagination.activate('approved');
            const s = Pagination.getState('approved');
            loadApproved(s.page, s.pageSize);
        }
        if (type === 'unapproved') {
            Pagination.activate('unapproved');
            const s = Pagination.getState('unapproved');
            loadUnapproved(s.page, s.pageSize);
        }
    });
});

// 4. Your loader functions — call Pagination.update() when data arrives
function loadApproved(page, pageSize) {
    loadingStart();
    fetch(`/Workstation/GetApprovedDetailsPaged?page=${page}&pageSize=${pageSize}`)
        .then(r => r.json())
        .then(result => {
            Pagination.update('approved', result.total, page, pageSize);
            // … render rows into #approvedtablebody …
        })
        .finally(() => loadingStop());
}

function loadUnapproved(page, pageSize) {
    loadingStart();
    fetch(`/Workstation/GetUnApprovedDetailsPaged?page=${page}&pageSize=${pageSize}`)
        .then(r => r.json())
        .then(result => {
            Pagination.update('unapproved', result.total, page, pageSize);
            // … render rows into #unapprovedtablebody …
        })
        .finally(() => loadingStop());
}

// 5. After approve/delete, reload the same page (not page 1):
function postapproveDevice(deviceId) {
    $.ajax({
        url: 'Workstation/ApproveDevice', type: 'GET', data: { ID: deviceId },
        success: function (response) {
            if (response) {
                const s = Pagination.getState('unapproved');
                loadUnapproved(s.page, s.pageSize);
            }
        }
    });
}

   ══════════════════════════════════════════════════════════════════════════════
   END OF USAGE EXAMPLE
   ══════════════════════════════════════════════════════════════════════════════ */
