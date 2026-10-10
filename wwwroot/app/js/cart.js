// Cart functionality
const Cart = {
    init: function () {
        this.bindEvents();
        this.updateCartCount(); // Initial check
    },

    bindEvents: function () {
        // Add to Cart Buttons
        $(document).on('click', '.btn-add-to-cart', function (e) {
            e.preventDefault();
            const btn = $(this);
            const styleCode = btn.data('stylecode');
            const quantity = btn.data('quantity') || 1;

            const form = btn.closest('form');
            let formQty = 1;
            if (form.length > 0) {
                const input = form.find('input[name="number"]'); // Theme uses name="number"
                if (input.length > 0) formQty = input.val();
            }

            Cart.addToCart(styleCode, parseInt(formQty) > 1 ? formQty : quantity, btn);
        });

        // Cart Page: Update Quantity
        $(document).on('change', '.cart-qty-input', function () {
            const input = $(this);
            if (input.prop('disabled')) return;

            const cartItemId = input.data('cartitemid');
            const styleCode = input.data('stylecode');
            const quantity = parseInt(input.val(), 10);

            if (isNaN(quantity) || quantity < 1) {
                input.val(1);
                return;
            }

            Cart.updateQuantity(cartItemId, styleCode, quantity, input);
        });

        // Cart Page: Plus/Minus Buttons
        $(document).on('click', '.js-cart-qty-btn', function (e) {
            e.preventDefault();
            const btn = $(this);
            const input = btn.siblings('input.cart-qty-input');
            if (input.prop('disabled')) return;

            let currentVal = parseInt(input.val(), 10) || 1;
            if (btn.hasClass('js-qty-plus')) {
                input.val(currentVal + 1).trigger('change');
            } else if (btn.hasClass('js-qty-minus') && currentVal > 1) {
                input.val(currentVal - 1).trigger('change');
            }
        });

        // Cart Page: Remove Item
        $(document).on('click', '.btn-remove-cart-item', function (e) {
            e.preventDefault();
            const btn = $(this);
            const cartItemId = btn.data('cartitemid');
            const styleCode = btn.data('stylecode');

            if (confirm('Are you sure you want to remove this item?')) {
                Cart.removeItem(cartItemId, styleCode, btn);
            }
        });

        // Sidebar: Remove Item
        $(document).on('click', '#shoppingCart .remove', function (e) {
            e.preventDefault();
            const btn = $(this);
            const cartItemId = btn.data('cartitemid');
            const styleCode = btn.data('stylecode');
            if (cartItemId) {
                Cart.removeItem(cartItemId, styleCode, btn);
            }
        });

        // Clear Cart
        $(document).on('click', '.btn-clear-cart', function (e) {
            e.preventDefault();
            if (confirm('Are you sure you want to empty your cart?')) {
                Cart.clearCart();
            }
        });
    },

    addToCart: function (styleCode, quantity, btnElement) {
        if (!styleCode) return;

        const originalText = btnElement.html();
        btnElement.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>');
        btnElement.prop('disabled', true);

        $.ajax({
            url: '/Cart/AddToCart',
            type: 'POST',
            data: { styleCode: styleCode, quantity: quantity },
            success: function (response) {
                if (response.success) {
                    Cart.updateAllUI(response);

                    // Open Sidebar
                    const sidebar = document.getElementById('shoppingCart');
                    if (sidebar) {
                        const bsOffcanvas = bootstrap.Offcanvas.getInstance(sidebar) || new bootstrap.Offcanvas(sidebar);
                        bsOffcanvas.show();
                    }
                } else {
                    alert(response.message);
                }
            },
            error: function () {
                alert('An error occurred.');
            },
            complete: function () {
                btnElement.html(originalText);
                btnElement.prop('disabled', false);
            }
        });
    },

    updateQuantity: function (cartItemId, styleCode, quantity, inputElement) {
        inputElement.prop('disabled', true);
        const row = inputElement.closest('tr');
        row.css('opacity', '0.6');

        $.ajax({
            url: '/Cart/UpdateQuantity',
            type: 'POST',
            data: { cartItemId: cartItemId, styleCode: styleCode, quantity: quantity },
            success: function (response) {
                if (response.success) {
                    Cart.updateAllUI(response);
                } else {
                    alert(response.message);
                    location.reload();
                }
            },
            error: function () {
                alert('Error updating cart.');
                location.reload();
            },
            complete: function () {
                inputElement.prop('disabled', false);
                row.css('opacity', '1');
            }
        });
    },

    removeItem: function (cartItemId, styleCode, btnElement) {
        $.ajax({
            url: '/Cart/RemoveItem',
            type: 'POST',
            data: { cartItemId: cartItemId, styleCode: styleCode },
            success: function (response) {
                if (response.success) {
                    Cart.updateAllUI(response);
                } else {
                    alert(response.message);
                }
            },
            error: function () {
                alert('Error removing item.');
            }
        });
    },

    clearCart: function () {
        $.ajax({
            url: '/Cart/ClearCart',
            type: 'POST',
            success: function (response) {
                if (response.success) {
                    Cart.updateAllUI(response);
                    if (window.location.pathname.toLowerCase().includes('/cart')) {
                        location.reload();
                    }
                }
            }
        });
    },

    updateAllUI: function (data) {
        // Update Indicator
        if (data.indicatorHtml) {
            $('#cart-indicator-container').html(data.indicatorHtml);
        }

        // Update Sidebar
        if (data.sidebarHtml) {
            $('#shoppingCart .canvas-wrapper').html(data.sidebarHtml);
        }

        // Update Cart Page Items
        if (data.itemsHtml) {
            $('#cart-items-container').html(data.itemsHtml);
        }

        // Update Cart Page Summary
        if (data.summaryHtml) {
            $('#cart-summary-container').html(data.summaryHtml);
        }

        // Fallback for counts if needed
        if (data.cartCount !== undefined) {
            $('.cart-count').text(data.cartCount); // Mobile toolbar
        }

        // If cart became empty on cart page and we didn't reload, show empty message
        if (data.cartCount === 0 && $('#cart-main-container').length > 0 && !data.itemsHtml) {
            location.reload();
        }
    },

    updateCartCount: function () {
        $.ajax({
            url: '/Cart/GetCartInfo',
            type: 'GET',
            success: function (response) {
                if (response.success) {
                    if (response.count !== undefined) {
                        $('.cart-count').text(response.count);
                    }
                }
            }
        });
    }
};

$(document).ready(function () {
    Cart.init();
});
