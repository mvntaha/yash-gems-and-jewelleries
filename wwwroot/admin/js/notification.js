/**
 * Notification System JavaScript
 * Real-time updates via SignalR
 */
"use strict";

const NotificationApp = (function () {
    let connection = null;

    const initSignalR = function () {
        if (typeof signalR === "undefined") {
            console.warn("SignalR library not loaded");
            return;
        }

        connection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationHub")
            .withAutomaticReconnect()
            .build();

        // On receiving a notification refresh, we can trigger a page-specific refresh if needed
        // For now, we'll just log it as the dropdown is removed
        connection.on("ReceiveNotificationRefresh", function () {
            console.log("New notification received. Refreshing page if on notifications index.");
            if (window.location.pathname.includes("/Admin/Notifications")) {
                location.reload();
            }
        });

        connection.start().catch(function (err) {
            console.error("SignalR connection error: ", err.toString());
        });
    };

    return {
        init: function () {
            initSignalR();
        }
    };
})();

$(document).ready(function () {
    NotificationApp.init();
});
