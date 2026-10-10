/**
 * Centralized Administrative Helper Functions
 * Standardizes Toast notifications and SweetAlert confirmations.
 */

window.AdminPanel = (function () {

    // showToast and handleResponse have been moved to admin-toast.js
    // for better centralization and TempData support.

    return {
        showToast: AdminToast.show,
        handleResponse: AdminToast.handleResponse,
        confirmAction: AdminToast.confirmAction
    };
})();
