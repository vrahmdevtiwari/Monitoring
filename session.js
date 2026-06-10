/// <reference path="../sweetalert2.all.min.js" />

// =============================================================================
// session.js — Client Projects (Monitoring, ITAM, ITSM, NonITAM, PatchMgmt)
//
// Strategy:
//   1. User activity detect karo (mouse, key, scroll, click)
//   2. BroadcastChannel se same-browser ke SSOUser tab ko activity notify karo
//   3. SSOUser ke proxy endpoint ko heartbeat bhejo (har 4 min — agar active ho)
//   4. Har 1 min mein SSOUser se check karo — kya session expire hua?
//   5. Agar expired → local logout
// =============================================================================

(function () {
    'use strict';

    // ---- Configuration ----
    // ✅ SSOUser ka base URL — _Layout.cshtml mein inject karo (neeche note dekho)
    //    Agar SSO_USER_BASE_URL global variable nahi mila to fallback use hoga.
    var SSO_USER_URL = (typeof SSO_USER_BASE_URL !== 'undefined' && SSO_USER_BASE_URL)
        ? SSO_USER_BASE_URL
        : 'https://localhost:7002'; // fallback — production mein _Layout se inject karo

    var HEARTBEAT_INTERVAL_MS = 4 * 60 * 1000;  // 4 minutes
    var CHECK_INTERVAL_MS = 1 * 60 * 1000;  // 1 minute
    var ACTIVITY_DEBOUNCE_MS = 2000;            // 2 seconds — bar bar heartbeat na jaye

    // ---- State ----
    var lastLocalActivity = Date.now();
    var heartbeatTimer = null;
    var checkTimer = null;
    var debounceTimer = null;
    var isLoggingOut = false;
    var warningShown = false;

    // ---- BroadcastChannel ----
    // Same browser ke sabhi tabs (SSOUser + Client tabs) ek channel share karte hain.
    // Cross-origin tabs direct communicate nahi kar sakti — isliye SSOServer proxy hai.
    var channel = null;
    try {
        channel = new BroadcastChannel('sso_activity_channel');
    } catch (e) {
        console.warn('[session.js] BroadcastChannel not supported. Server-only sync will be used.');
    }

    // ---- Activity Events ----
    var activityEvents = ['mousemove', 'keypress', 'scroll', 'click', 'touchstart'];

    function onUserActivity() {
        lastLocalActivity = Date.now();
        warningShown = false;

        // BroadcastChannel se SSOUser tab ko notify karo (agar same browser mein hai)
        if (channel) {
            try {
                channel.postMessage({ type: 'activity', timestamp: lastLocalActivity });
            } catch (e) { /* ignore */ }
        }

        // Debounce — 2 sec mein baar baar heartbeat nahi jayega
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(function () {
            sendHeartbeat();
        }, ACTIVITY_DEBOUNCE_MS);
    }

    activityEvents.forEach(function (eventName) {
        document.addEventListener(eventName, onUserActivity, { passive: true });
    });

    // ---- BroadcastChannel Listener ----
    if (channel) {
        channel.onmessage = function (event) {
            if (!event || !event.data) return;

            if (event.data.type === 'logout') {
                // SSOUser ya koi aur tab ne logout kiya — hum bhi logout karo
                performLogout();
            }
            // activity message receive hone par koi action nahi — heartbeat SSOUser hi bhejta hai
        };
    }

    // ---- Heartbeat — SSOUser proxy ke through SSOServer ko ----
    function sendHeartbeat() {
        if (isLoggingOut) return;

        fetch(SSO_USER_URL + '/api/activity/heartbeat', {
            method: 'POST',
            credentials: 'include',   // SSOUser ka cookie (user) bhi jayega
            headers: {
                'Content-Type': 'application/json'
            }
        })
            .then(function (response) {
                if (response.status === 401) {
                    // SSOUser ka session bhi expire ho gaya
                    performLogout();
                }
                // 200 OK — heartbeat successful, koi action nahi
            })
            .catch(function (err) {
                // Network error — silently ignore, next interval par retry hoga
                console.warn('[session.js] Heartbeat failed:', err);
            });
    }

    // ---- Periodic Server Check ----
    function checkSessionStatus() {
        if (isLoggingOut) return;

        fetch(SSO_USER_URL + '/api/activity/check', {
            method: 'GET',
            credentials: 'include'
        })
            .then(function (response) {
                if (response.status === 401) {
                    performLogout();
                    return null;
                }
                return response.json();
            })
            .then(function (data) {
                if (!data) return;

                if (data.isExpired === true) {
                    performLogout();
                    return;
                }

                // Warning — 2 min remaining aur abhi tak warning nahi dikhi
                if (data.remainingMinutes !== undefined &&
                    data.remainingMinutes <= 2 &&
                    !warningShown) {
                    warningShown = true;
                    showTimeoutWarning(data.remainingMinutes);
                }
            })
            .catch(function (err) {
                console.warn('[session.js] Session check failed:', err);
            });
    }

    // ---- Warning Popup ----
    function showTimeoutWarning(remainingMinutes) {
        if (typeof Swal === 'undefined') return;

        Swal.fire({
            title: 'Session Expiring Soon',
            text: 'Aapka session ' + (remainingMinutes || 2) + ' minute(s) mein expire ho jayega. '
                + 'Koi bhi action karein session extend karne ke liye.',
            icon: 'warning',
            timer: (remainingMinutes || 2) * 60 * 1000,
            timerProgressBar: true,
            showConfirmButton: true,
            confirmButtonText: 'Continue Session'
        }).then(function (result) {
            if (result.isConfirmed || result.dismiss) {
                // User ne popup ke saath interact kiya — activity treat karo
                onUserActivity();
            }
        });
    }

    // ---- Logout ----
    function performLogout() {
        if (isLoggingOut) return;
        isLoggingOut = true;

        clearInterval(heartbeatTimer);
        clearInterval(checkTimer);
        clearTimeout(debounceTimer);

        // Doosre tabs ko bhi batao logout karo
        if (channel) {
            try {
                channel.postMessage({ type: 'logout' });
            } catch (e) { /* ignore */ }
        }

        // Client project ka apna logout
        var form = document.createElement('form');
        form.method = 'POST';
        form.action = '/Account/Signout';
        document.body.appendChild(form);
        form.submit();
    }

    // ---- Init ----
    // Page load par turant pehla heartbeat bhejo
    sendHeartbeat();

    // Har 4 min mein — sirf agar last 4 min mein activity thi
    heartbeatTimer = setInterval(function () {
        if (Date.now() - lastLocalActivity < HEARTBEAT_INTERVAL_MS) {
            sendHeartbeat();
        }
    }, HEARTBEAT_INTERVAL_MS);

    // Har 1 min mein server se check karo
    checkTimer = setInterval(checkSessionStatus, CHECK_INTERVAL_MS);

})();

// =============================================================================
// _Layout.cshtml mein is tarah inject karo (RECOMMENDED):
//
//   <script>
//       var SSO_USER_BASE_URL = '@Configuration["SSOServer:Welcome"]';
//   </script>
//   <script src="~/js/session.js"></script>
//
// Isse SSO_USER_URL automatically appsettings se aayega — hardcode nahi karna.
// =============================================================================




// /// <reference path="../sweetalert2.all.min.js" />

// // wwwroot/js/session.js
// let $idleTimeout = 20 * 60 * 1000; // 20 minutes
// let timeoutWarning = 18 * 60 * 1000; // 18 minutes
// let timeout;
// let warningTimeout;

// function resetTimer() {
//     // Clear both the logout and warning timeouts
//     clearTimeout(timeout);
//     clearTimeout(warningTimeout);
//     //// If a warning popup is currently visible, close it
//     //if (Swal.isVisible()) {
//     //    Swal.close();
//     //}
//     // Set new timeouts
//     timeout = setTimeout(logoutUser, $idleTimeout);
//     warningTimeout = setTimeout(showTimeoutWarning, timeoutWarning);
// }

// function showTimeoutWarning() {
//     Swal.fire({
//         title: 'Session Expiring',
//         text: `Your session will expire in ${($idleTimeout - timeoutWarning) / 60000} minutes`,
//         icon: 'warning',
//         timer: 120000, // Popup auto-closes after 2 minutes
//         timerProgressBar: true  // Optional: shows a progress bar for the timer
//     });
// }

// function logoutUser() {
//     // Create a form dynamically
//     var form = document.createElement('form');
//     form.method = 'POST';
//     form.action = '/Account/Signout';  // POST to the /Account/Signout endpoint

//     // Append the form to the body and submit it
//     document.body.appendChild(form);
//     form.submit();
// }

// // Reset timer on any of these events
// window.onload = resetTimer;
// window.onmousemove = resetTimer;
// window.onkeypress = resetTimer;
// window.onscroll = resetTimer;
// window.onclick = resetTimer;