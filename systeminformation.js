/// <reference path="../../lib/jquery/dist/jquery.js" />

async function getSystemInfo() {
    const systemInfo = {
        platform: navigator.platform,
        userAgent: navigator.userAgent,
        language: navigator.language,
        browserName: getBrowserName(),
        browserVersion: getBrowserVersion(),
        screenResolution: `${screen.width}x${screen.height}`,
        deviceMemory: navigator.deviceMemory || 'Unknown',
        hardwareConcurrency: navigator.hardwareConcurrency || 'Unknown',
        deviceType: inferDeviceType(),
        ...getOSDetails()
    };

    // IP and Location Info (IPv4 only)
    let ipInfo = {};
    try {
        const response = await fetch('https://ipapi.co/json/?ipv4');
        if (response.ok) {
            const data = await response.json();
            ipInfo = {
                ip: data.ip,
                city: data.city,
                region: data.region,
                country: data.country_name,
                latitude: data.latitude,
                longitude: data.longitude,
                org: data.org,
            };
        } else {
            console.error('Failed to fetch IP info');
        }
    } catch (error) {
        console.error('Error fetching IP info:', error);
    }

    const fullInfo = {
        'BrowserName': systemInfo.browserName,
        'BrowserVersion': systemInfo.browserVersion,
        'DeviceMemory': systemInfo.deviceMemory,
        'DeviceType': systemInfo.deviceType,
        'HardwareConcurrency': systemInfo.hardwareConcurrency,
        'Language': systemInfo.language,
        'OsManufacturer': systemInfo.osManufacturer,
        'OsName': systemInfo.osName,
        'OsVersion': systemInfo.osVersion,
        'Platform': systemInfo.platform,
        'ScreenResolution': systemInfo.screenResolution,
        'UserAgent': systemInfo.userAgent,
        'IP': ipInfo.ip,
        'Latitude': ipInfo.latitude,
        'Longitude': ipInfo.longitude,
        'Org': ipInfo.org,
        'region': ipInfo.region,
        'City': ipInfo.city,
        'Country': ipInfo.country
    };

    console.log("✅ Collected Device & System Info:", fullInfo);
    return fullInfo;
}

function getBrowserName() {
    const ua = navigator.userAgent;
    if (ua.includes("Firefox")) return "Firefox";
    if (ua.includes("Edg")) return "Edge";
    if (ua.includes("Chrome") && !ua.includes("Edg")) return "Chrome";
    if (ua.includes("Safari") && !ua.includes("Chrome")) return "Safari";
    return "Unknown";
}

function getBrowserVersion() {
    const ua = navigator.userAgent;
    const match = ua.match(/(Chrome|Firefox|Edg|Safari)\/([\d.]+)/);
    return match ? match[2] : "Unknown";
}

function inferDeviceType() {
    const ua = navigator.userAgent;
    const width = screen.width;

    if (/Mobi|Android|iPhone|iPad|iPod/i.test(ua)) {
        return "Mobile";
    }

    if (width <= 768) {
        return "Tablet";
    }

    return "Desktop";
}

function getOSDetails() {
    const ua = navigator.userAgent;
    let os = "Unknown";
    let version = "Unknown";

    if (ua.includes("Windows NT 10.0")) {
        os = "Windows";
        version = "10";
    } else if (ua.includes("Windows NT 6.3")) {
        os = "Windows";
        version = "8.1";
    } else if (ua.includes("Windows NT 6.2")) {
        os = "Windows";
        version = "8";
    } else if (ua.includes("Windows NT 6.1")) {
        os = "Windows";
        version = "7";
    } else if (/Mac OS X (\d+[\._]\d+)/.test(ua)) {
        os = "macOS";
        version = ua.match(/Mac OS X (\d+[\._]\d+)/)[1].replace('_', '.');
    } else if (/Android (\d+[\.\d]*)/.test(ua)) {
        os = "Android";
        version = ua.match(/Android (\d+[\.\d]*)/)[1];
    } else if (/iPhone OS (\d+_\d+)/.test(ua)) {
        os = "iOS";
        version = ua.match(/iPhone OS (\d+_\d+)/)[1].replace('_', '.');
    } else if (ua.includes("Linux")) {
        os = "Linux";
    }

    return {
        osName: os,
        osVersion: version,
        osManufacturer: getOSManufacturer(os),
    };
}

function getOSManufacturer(os) {
    switch (os) {
        case "Windows":
            return "Microsoft";
        case "macOS":
        case "iOS":
            return "Apple";
        case "Android":
            return "Google";
        case "Linux":
            return "Open Source";
        default:
            return "Unknown";
    }
}

// Run the main function on load
getSystemInfo();
