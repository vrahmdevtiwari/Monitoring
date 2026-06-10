// kpis.js — Windows Patch KPI Dashboard

const COLORS = [
    "#378ADD", "#1D9E75", "#BA7517", "#534AB7", "#E24B4A",
    "#639922", "#D4537E", "#0F6E56", "#993C1D", "#3C3489",
    "#185FA5", "#3B6D11", "#854F0B", "#A32D2D", "#72243E",
    "#D85A30", "#0F7350", "#72243E", "#27500A", "#633806"
];
const getColor = i => COLORS[i % COLORS.length];

let statusChartInstance = null;
let platformChartInstance = null;
let categoryChartInstance = null;
let vendorChartInstance = null;
let osVersionChartInstance = null;   // ← NEW
let archChartInstance = null;   // ← NEW

// ── Safe accessor: handles both camelCase and PascalCase ──────
function g(obj, camel, pascal) {
    if (!obj) return null;
    return obj[camel] !== undefined ? obj[camel]
        : obj[pascal] !== undefined ? obj[pascal]
            : null;
}
function safeNum(v) { return v ?? 0; }

function getPieHeight(count) {
    return Math.min(Math.max(count * 35, 200), 280) + "px";
}
function getBarHeight(count) {
    return Math.min(Math.max(count * 45, 200), 320) + "px";
}

// ── Toggle chart vs no-data message ──────────────────────────
function toggleChart(prefix, isEmpty) {
    var wrap = document.getElementById(prefix + "-chart-wrap");
    var noData = document.getElementById(prefix + "-no-data");
    if (isEmpty) {
        if (wrap) wrap.style.display = "none";
        if (noData) noData.style.display = "block";
    } else {
        if (wrap) wrap.style.display = "block";
        if (noData) noData.style.display = "none";
    }
}

// ── Legend ────────────────────────────────────────────────────
function renderLegend(containerId, items, fixedColors) {
    var el = document.getElementById(containerId);
    if (!el || !items || items.length === 0) return;
    el.innerHTML = items.map(function (item, i) {
        var color = fixedColors ? fixedColors[i] : getColor(i);
        var name = g(item, "name", "Name") || "Unknown";
        var count = g(item, "count", "Count");
        return '<span class="legend-item">'
            + '<span class="legend-dot" style="background:' + color + ';"></span>'
            + name + (count !== null ? ' (' + count + ')' : '')
            + '</span>';
    }).join("");
}

// ── Normalize: same name different case → merge into PascalCase ──
function normalizeList(list) {
    if (!list || list.length === 0) return [];

    var map = {};  // lowercase key → { name: PascalCase, count: total }

    list.forEach(function (item) {
        var rawName = g(item, "name", "Name") || "Unknown";
        var rawCount = g(item, "count", "Count") || 0;
        var key = rawName.trim().toLowerCase();  // case-insensitive grouping key

        // PascalCase banao — pehla letter capital, baaki lowercase
        var pascalName = rawName.trim().charAt(0).toUpperCase()
            + rawName.trim().slice(1).toLowerCase();

        if (map[key]) {
            map[key].count += rawCount;  // same name → count add karo
        } else {
            map[key] = { name: pascalName, count: rawCount };
        }
    });

    // Object to array convert karo
    return Object.values(map);
}

// -- Pie chart -------------------------------------------------
function renderPieChart(canvasId, wrapId, items, oldInstance, dynamicHeight) {
    if (oldInstance) { try { oldInstance.destroy(); } catch (e) { } }
    if (!items || items.length === 0) return null;

    var wrap = document.getElementById(wrapId);
    if (wrap) {
        wrap.style.display = "block";
        if (dynamicHeight) wrap.style.height = getPieHeight(items.length);
    }

    var labels = items.map(function (x) { return g(x, "name", "Name") || "Unknown"; });
    var counts = items.map(function (x) { return g(x, "count", "Count") || 0; });
    var bgColors = items.map(function (_, i) { return getColor(i); });

    var canvas = document.getElementById(canvasId);
    if (!canvas) return null;

    return new Chart(canvas, {
        type: "pie",
        data: {
            labels: labels,
            datasets: [{
                data: counts,
                backgroundColor: bgColors,
                borderWidth: 2,
                borderColor: "#fff",
                hoverOffset: 6
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            //cutout: "65%",
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        label: function (ctx) {
                            var total = ctx.dataset.data.reduce(function (a, b) { return a + b; }, 0);
                            var pct = total > 0 ? Math.round((ctx.parsed / total) * 100) : 0;
                            return " " + ctx.label + ": " + ctx.parsed + " (" + pct + "%)";
                        }
                    }
                }
            }
        }
    });
}

// ── Donut chart ───────────────────────────────────────────────
function renderDonutChart(canvasId, wrapId, items, oldInstance, dynamicHeight) {
    if (oldInstance) { try { oldInstance.destroy(); } catch (e) { } }
    if (!items || items.length === 0) return null;

    var wrap = document.getElementById(wrapId);
    if (wrap) {
        wrap.style.display = "block";
        if (dynamicHeight) wrap.style.height = getPieHeight(items.length);
    }

    var labels = items.map(function (x) { return g(x, "name", "Name") || "Unknown"; });
    var counts = items.map(function (x) { return g(x, "count", "Count") || 0; });
    var bgColors = items.map(function (_, i) { return getColor(i); });

    var canvas = document.getElementById(canvasId);
    if (!canvas) return null;

    return new Chart(canvas, {
        type: "doughnut",
        data: {
            labels: labels,
            datasets: [{
                data: counts,
                backgroundColor: bgColors,
                borderWidth: 2,
                borderColor: "#fff",
                hoverOffset: 6
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: "50%",
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        label: function (ctx) {
                            var total = ctx.dataset.data.reduce(function (a, b) { return a + b; }, 0);
                            var pct = total > 0 ? Math.round((ctx.parsed / total) * 100) : 0;
                            return " " + ctx.label + ": " + ctx.parsed + " (" + pct + "%)";
                        }
                    }
                }
            }
        }
    });
}

// ── Vertical bar chart ────────────────────────────────────────
function renderBarChart(canvasId, wrapId, items, oldInstance, fixedColors, dynamicHeight) {
    if (oldInstance) { try { oldInstance.destroy(); } catch (e) { } }
    if (!items || items.length === 0) return null;

    var wrap = document.getElementById(wrapId);
    if (wrap) {
        wrap.style.display = "block";
        if (dynamicHeight) wrap.style.height = getBarHeight(items.length);
    }

    var labels = items.map(function (x) { return g(x, "name", "Name") || "Unknown"; });
    var counts = items.map(function (x) { return g(x, "count", "Count") || 0; });
    var bgColors = fixedColors ? fixedColors : items.map(function (_, i) { return getColor(i); });

    var canvas = document.getElementById(canvasId);
    if (!canvas) return null;

    return new Chart(canvas, {
        type: "bar",
        data: {
            labels: labels,
            datasets: [{
                data: counts,
                backgroundColor: bgColors,
                borderRadius: 5,
                borderSkipped: false,
                maxBarThickness: 48,
                barPercentage: 0.6,
                categoryPercentage: 0.7
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        label: function (ctx) { return " " + ctx.parsed.y + " patches"; }
                    }
                }
            },
            scales: {
                x: {
                    grid: { display: false },
                    ticks: {
                        font: { size: 11 },
                        maxRotation: items.length > 5 ? 45 : 0,
                        autoSkip: false
                    }
                },
                y: {
                    grid: { color: "rgba(0,0,0,0.06)" },
                    beginAtZero: true,
                    ticks: { stepSize: 1, font: { size: 11 } }
                }
            }
        }
    });
}

// ── Horizontal bar chart ──────────────────────────────────────
function renderHorizontalBarChart(canvasId, wrapId, items, oldInstance, dynamicHeight) {
    if (oldInstance) { try { oldInstance.destroy(); } catch (e) { } }
    if (!items || items.length === 0) return null;

    var wrap = document.getElementById(wrapId);
    if (wrap) {
        wrap.style.display = "block";
        if (dynamicHeight) wrap.style.height = getBarHeight(items.length);
    }

    var labels = items.map(function (x) { return g(x, "name", "Name") || "Unknown"; });
    var counts = items.map(function (x) { return g(x, "count", "Count") || 0; });
    var bgColors = items.map(function (_, i) { return getColor(i); });

    var canvas = document.getElementById(canvasId);
    if (!canvas) return null;

    // Sort descending so largest bar is at top (Google-style "Top-X")
    var combined = labels.map(function (l, i) { return { label: l, count: counts[i], color: bgColors[i] }; });
    combined.sort(function (a, b) { return b.count - a.count; });

    return new Chart(canvas, {
        type: "bar",
        data: {
            labels: combined.map(function (x) { return x.label; }),
            datasets: [{
                data: combined.map(function (x) { return x.count; }),
                backgroundColor: combined.map(function (x) { return x.color; }),
                borderRadius: 4,
                borderSkipped: false,
                maxBarThickness: 28,
                barPercentage: 0.7,
                categoryPercentage: 0.8
            }]
        },
        options: {
            indexAxis: "y",                  // ← makes it horizontal
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        label: function (ctx) {
                            var total = ctx.dataset.data.reduce(function (a, b) { return a + b; }, 0);
                            var pct = total > 0 ? Math.round((ctx.parsed.x / total) * 100) : 0;
                            return " " + ctx.parsed.x + " (" + pct + "%)";
                        }
                    }
                }
            },
            scales: {
                x: {
                    beginAtZero: true,
                    grid: { color: "rgba(0,0,0,0.06)" },
                    ticks: { font: { size: 11 }, stepSize: 1 }
                },
                y: {
                    grid: { display: false },
                    ticks: {
                        font: { size: 11 },
                        // Truncate long labels so they don't overflow
                        callback: function (val, index) {
                            var label = this.getLabelForValue(index);
                            return label.length > 18 ? label.substring(0, 16) + "…" : label;
                        }
                    }
                }
            }
        }
    });
}

// ── Main render ───────────────────────────────────────────────
function renderDashboard(data) {
    console.log("=== renderDashboard called ===");
    console.log("Raw data:", JSON.stringify(data));

    document.getElementById("loading-state").style.display = "none";
    document.getElementById("dashboard").style.display = "block";

    // ── Summary card values ───────────────────────────────────
    var reported = safeNum(g(data, "reportedDevices", "ReportedDevices"));
    var approved = safeNum(g(data, "approvedDevices", "ApprovedDevices"));
    var unapproved = safeNum(g(data, "unApprovedDevices", "UnApprovedDevices"));
    var total = safeNum(g(data, "total", "Total"));
    var failed = safeNum(g(data, "failed", "Failed"));
    var success = safeNum(g(data, "success", "Success"));
    var alreadyInstalled = safeNum(g(data, "alreadyInstalled", "AlreadyInstalled"));
    var missing = safeNum(g(data, "missing", "Missing"));

    document.getElementById("kpi-total-reported-devices").innerText = reported;
    document.getElementById("kpi-approved-devices").innerText = approved;
    document.getElementById("kpi-unapproved-devices").innerText = unapproved;
    document.getElementById("kpi-total").innerText = total;
    document.getElementById("kpi-success").innerText = success;
    document.getElementById("kpi-failed").innerText = failed;
    document.getElementById("kpi-missing").innerText = missing;
    document.getElementById("kpi-reboot").innerText = alreadyInstalled;

    // ── Status bar chart ──────────────────────────────────────
    var statusItems = [
        { name: "Failed", count: failed },
        { name: "Success", count: success },
        { name: "Already installed", count: alreadyInstalled }
    ];
    var statusColors = ["#E24B4A", "#1D9E75", "#378ADD"];

    if (statusChartInstance) { try { statusChartInstance.destroy(); } catch (e) { } }
    statusChartInstance = new Chart(document.getElementById("statusChart"), {
        type: "bar",
        data: {
            labels: ["Failed", "Success", "Already installed"],
            datasets: [{
                data: [failed, success, alreadyInstalled],
                backgroundColor: statusColors,
                borderRadius: 6,
                borderSkipped: false,
                maxBarThickness: 60,
                barPercentage: 0.5,
                categoryPercentage: 0.6
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        label: function (ctx) { return " " + ctx.parsed.y + " patches"; }
                    }
                }
            },
            scales: {
                x: { grid: { display: false }, ticks: { font: { size: 12 } } },
                y: { grid: { color: "rgba(0,0,0,0.06)" }, beginAtZero: true, ticks: { stepSize: 1 } }
            }
        }
    });
    renderLegend("status-legend", statusItems, statusColors);

    // ── Resolve missingPatches object ─────────────────────────
    var mp = g(data, "missingPatches", "MissingPatches");
    console.log("missingPatches resolved:", mp);

    // ── OS Version — BAR (middle row) ─────────────────────────  ← NEW
    /*var osVersionList = mp ? (g(mp, "osVersionList", "OSVersionList") || []) : [];*/ /*Replaced with below line of code*/
    var osVersionList = normalizeList(mp ? (g(mp, "osVersionList", "OSVersionList") || []) : []);
    var hasOsVersion = osVersionList.length > 0;
    toggleChart("osversion", !hasOsVersion);
    if (hasOsVersion) {
        osVersionChartInstance = renderBarChart(
            "osVersionChart", "osversion-chart-wrap",
            osVersionList, osVersionChartInstance,
            osVersionList.map(function (_, i) { return getColor(i); }),
            false   // fixed 180px height — do NOT use dynamic height
        );
        renderLegend("osversion-legend", osVersionList, null);
    }

    // ── Architecture — DONUT (middle row) ────────────────────  ← NEW
    /*var archList = mp ? (g(mp, "architectureList", "ArchitectureList") || []) : [];*/ /*Replaced with below line of code*/
    var archList = normalizeList(mp ? (g(mp, "architectureList", "ArchitectureList") || []) : []);
    var hasArch = archList.length > 0;
    toggleChart("arch", !hasArch);
    if (hasArch) {
        archChartInstance = renderHorizontalBarChart(
            "archChart", "arch-chart-wrap",
            archList, archChartInstance,
            false   // fixed 180px height
        );
        renderLegend("arch-legend", archList, null);
    }

    // ── No missingPatches → hide bottom row charts ────────────
    if (!mp) {
        console.warn("missingPatches is null/undefined — showing no data for bottom row");
        toggleChart("platform", true);
        toggleChart("category", true);
        toggleChart("vendor", true);
        return;
    }

    // ── Resolve bottom-row lists ──────────────────────────────
    /*var platformList = g(mp, "platformList", "PlatformList") || [];*/ /*Replaced with below line of code*/
    var platformList = normalizeList(g(mp, "platformList", "PlatformList") || []);
    /*var categoryList = g(mp, "categoryList", "CategoryList") || [];*/ /*Replaced with below line of code*/
    var categoryList = normalizeList(g(mp, "categoryList", "CategoryList") || []);
    /*var vendorList = g(mp, "vendorList", "VendorList") || [];*/ /*Replaced with below line of code*/
    var vendorList = normalizeList(g(mp, "vendorList", "VendorList") || []);

    console.log("platformList:", platformList);
    console.log("categoryList:", categoryList);
    console.log("vendorList:", vendorList);

    // ── Platform — DONUT ─────────────────────────────────────
    var hasPlatform = platformList.length > 0;
    toggleChart("platform", !hasPlatform);
    if (hasPlatform) {
        platformChartInstance = renderDonutChart(
            "platformChart", "platform-chart-wrap",
            platformList, platformChartInstance, true
        );
        renderLegend("platform-legend", platformList, null);
    }

    // ── Category — BAR ────────────────────────────────────────
    var hasCategory = categoryList.length > 0;
    toggleChart("category", !hasCategory);
    if (hasCategory) {
        categoryChartInstance = renderBarChart(
            "categoryChart", "category-chart-wrap",
            categoryList, categoryChartInstance, null, true
        );
        renderLegend("category-legend", categoryList, null);
    }

    // ── Vendor — DONUT ───────────────────────────────────────
    var hasVendor = vendorList.length > 0;
    toggleChart("vendor", !hasVendor);
    if (hasVendor) {
        vendorChartInstance = renderDonutChart(
            "vendorChart", "vendor-chart-wrap",
            vendorList, vendorChartInstance, true
        );
        renderLegend("vendor-legend", vendorList, null);
    }
}

// ── Fetch ─────────────────────────────────────────────────────
async function loadKPIData() {
    try {
        var response = await fetch("/Home/GetKpis", {
            method: "GET",
            headers: { "Content-Type": "application/json" }
        });

        if (!response.ok)
            throw new Error(response.status + " " + response.statusText);

        var data = await response.json();

        console.log("=== FULL RAW RESPONSE ===");
        console.log(JSON.stringify(data, null, 2));

        renderDashboard(data);

    } catch (err) {
        document.getElementById("loading-state").style.display = "none";
        document.getElementById("error-state").style.display = "block";
        document.getElementById("error-text").innerText = "Failed to load patch data: " + err.message;
        console.error("loadKPIData error:", err);
    }
}

document.addEventListener("DOMContentLoaded", function () {
    loadKPIData();
});
