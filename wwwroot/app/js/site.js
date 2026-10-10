// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(function () {
    initWishlist();
});

function initWishlist() {
    // Determine if we are on the wishlist page
    const isWishlistPage = $('#wishlist-main-container').length > 0;

    // Toggle Wishlist (Add/Remove) - for product cards anywhere
    $(document).on('click', '.btn-add-wishlist', function (e) {
        e.preventDefault();
        var $btn = $(this);
        var styleCode = $btn.data('stylecode');

        if (!styleCode) return;

        $.ajax({
            url: '/Wishlist/ToggleWishlist',
            type: 'POST',
            data: { styleCode: styleCode },
            success: function (response) {
                if (response.success) {
                    // Update all buttons for this product across the page
                    var $allBtns = $('.btn-add-wishlist[data-stylecode="' + styleCode + '"]');

                    if (response.added) {
                        $allBtns.addClass('active');
                        $allBtns.find('.icon').removeClass('icon-heart-2').addClass('icon-trash');
                        $allBtns.attr('title', 'Remove from Wishlist');
                        $allBtns.find('.tooltip').text('Remove from Wishlist');
                    } else {
                        $allBtns.removeClass('active');
                        $allBtns.find('.icon').removeClass('icon-trash').addClass('icon-heart-2');
                        $allBtns.attr('title', 'Add to Wishlist');
                        $allBtns.find('.tooltip').text('Add to Wishlist');
                    }

                    // Update Wishlist Count & Indicator
                    updateWishlistUI(response.count, response.indicatorHtml);

                    // If we are on the wishlist page and removed an item, refresh the list
                    if (isWishlistPage && !response.added) {
                        location.reload(); // Simple reload for now to reflect state on Wishlist page
                    }

                } else {
                    if (response.message === "User not logged in") {
                        window.location.href = '/Account/Login';
                    } else {
                        alert(response.message);
                    }
                }
            },
            error: function () {
                console.error('Error toggling wishlist');
            }
        });
    });

    // Remove from Wishlist Page (X button)
    $(document).on('click', '.remove[data-wishlistid]', function (e) {
        e.preventDefault();
        var $btn = $(this);
        var wishlistId = $btn.data('wishlistid');

        // Get AntiForgery Token
        var token = $('input[name="__RequestVerificationToken"]').val();

        $.ajax({
            url: '/Wishlist/RemoveFromWishlist',
            type: 'POST',
            data: {
                id: wishlistId,
                __RequestVerificationToken: token
            },
            success: function (response) {
                if (response.success) {
                    if (response.count === 0) {
                        location.reload(); // Reload to show empty state
                    } else {
                        // Replace the grid with updated items
                        // Assuming the server returns the updated partial view HTML for items
                        // Modify 'RemoveFromWishlist' to return partial view string if not already

                        // For now, simpler to remove the element from DOM
                        $btn.closest('.card_product--V01').fadeOut(300, function () {
                            $(this).remove();
                        });
                    }
                    updateWishlistUI(response.count, response.indicatorHtml);
                } else {
                    if (response.message === "User not logged in") {
                        window.location.href = '/Account/Login';
                    } else {
                        alert(response.message);
                    }
                }
            },
            error: function () {
                console.error('Error removing from wishlist');
            }
        });
    });
}

function updateWishlistUI(count, indicatorHtml) {
    // Update Mobile Toolbar Count
    $('.wishlist-count').text(count);

    // Update Desktop Header Indicator
    if (indicatorHtml) {
        $('#wishlist-indicator-container').html(indicatorHtml);
    }
}
