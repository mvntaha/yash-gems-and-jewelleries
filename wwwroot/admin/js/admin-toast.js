/**
 * Admin Panel - Global Toast System
 * Centralized handling for TempData and AJAX notifications.
 */

"use strict";

const AdminToast = (function () {
    /**
     * Shows a standardized Toast notification
     * @param {string} message - Message to display
     * @param {string} type - 'success', 'error', 'warning', 'info'
     * @param {object} options - Optional overrides (duration, position, etc.)
     */
    const show = function (message, type = 'success', options = {}) {
        if (!message) return;

        const bgClassMap = {
            'success': 'success',
            'error': 'danger',
            'danger': 'danger',
            'warning': 'warning',
            'info': 'info'
        };

        const themeClass = bgClassMap[type] || 'primary';

        if (typeof Toastify === 'function') {
            Toastify({
                text: message,
                duration: options.duration || 3000,
                close: options.close !== false,
                gravity: options.gravity || "top",
                position: options.position || "right",
                className: "bg-" + themeClass,
                stopOnFocus: true,
                offset: {
                    x: 10,
                    y: 10
                }
            }).showToast();
        } else {
            console.warn('Toastify library not found. Falling back to native notification.');
            // Fallback for extreme cases (if Toastify fails to load)
            alert(message);
        }
    };

    /**
     * Initializes toasts injected into the DOM via server-side TempData
     */
    const initTempDataToasts = function () {
        const toastElements = document.querySelectorAll('[data-admin-toast]');
        toastElements.forEach(el => {
            const message = el.getAttribute('data-message');
            const type = el.getAttribute('data-type') || 'success';
            if (message) {
                show(message, type);
                // Clean up to prevent duplicate firing on partial refreshes if somehow re-init
                el.removeAttribute('data-admin-toast');
            }
        });
    };

    /**
     * Universal AJAX response handler
     * Automatically shows toasts for structured responses: { success: true, message: "...", type: "success" }
     * @param {object} response - The server response
     * @param {function} onSuccess - Optional callback if success is true
     */
    const handleResponse = function (response, onSuccess) {
        if (response && typeof response === 'object') {
            if (response.success === true || response.success === undefined) {
                if (response.message) {
                    show(response.message, response.type || 'success');
                }
                if (onSuccess) onSuccess(response);
            } else {
                show(response.message || 'An error occurred during the operation.', response.type || 'danger');
            }
        } else if (onSuccess) {
            // Probably raw HTML or partial view
            onSuccess(response);
        }
    };

    /**
     * Universal confirmation dialog handler using SweetAlert2
     * @param {object} options - Custom options (title, text, confirmButtonText, confirmButtonColor, icon)
     * @param {function} onConfirm - Callback function executed only if user confirms
     */
    const confirmAction = function (options = {}, onConfirm) {
        if (typeof Swal !== 'function') {
            console.error('SweetAlert2 (Swal) not found. Falling back to native confirm.');
            if (confirm(options.text || 'Are you sure?')) {
                if (onConfirm) onConfirm();
            }
            return;
        }

        const defaultOptions = {
            title: options.title || 'Are you sure?',
            text: options.text || "You won't be able to revert this!",
            icon: options.icon || 'warning',
            showCancelButton: true,
            confirmButtonText: options.confirmButtonText || 'Yes, proceed!',
            cancelButtonText: options.cancelButtonText || 'No, cancel!',
            confirmButtonClass: 'btn btn-primary w-xs me-2 mt-2',
            cancelButtonClass: 'btn btn-danger w-xs mt-2',
            buttonsStyling: false,
            showCloseButton: false
        };

        // If a specific confirmButtonColor is provided (legacy support for some views)
        // we might need to adjust styling, but usually classes are preferred in this theme.

        Swal.fire({ ...defaultOptions, ...options }).then(function (result) {
            if (result.isConfirmed || result.value) {
                if (onConfirm) onConfirm();
            }
        });
    };

    return {
        show: show,
        init: initTempDataToasts,
        handleResponse: handleResponse,
        confirmAction: confirmAction
    };
})();

// Auto-initialize server-side toasts on document ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => AdminToast.init());
} else {
    AdminToast.init();
}

// Export for global usage
window.AdminToast = AdminToast;
// Also alias to AdminPanel if it exists or create it for compatibility
window.AdminPanel = window.AdminPanel || {};
window.AdminPanel.showToast = AdminToast.show;
window.AdminPanel.handleResponse = AdminToast.handleResponse;
window.AdminPanel.confirmAction = AdminToast.confirmAction;
