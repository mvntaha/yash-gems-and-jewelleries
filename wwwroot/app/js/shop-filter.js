$(document).ready(function () {
    const filterForm = $('#filterForm');
    const productGrid = $('#gridLayout');
    const appliedFiltersTop = $('#applied-filters');
    const appliedFiltersSidebar = $('#applied-filters-sidebar');
    const totalCountDisplay = $('#total-count-display');

    renderFilterTags(false); // Initial render for top bar

    // Intercept Form Submission (Apply Button)
    filterForm.on('submit', function (e) {
        e.preventDefault();
        $('#pageInput').val(1); // Reset to page 1 on new filter
        updateProducts();

        const offcanvas = bootstrap.Offcanvas.getInstance(document.getElementById('filterShop'));
        if (offcanvas) offcanvas.hide();
    });

    // Queue Selections (Sidebar only)
    filterForm.on('change', 'input[type="checkbox"], input[type="radio"]', function () {
        // If it's a price radio, sync the hidden inputs immediately for the pending state
        if ($(this).hasClass('price-radio')) {
            const val = $(this).val();
            if (val) {
                const parts = val.split('-');
                $('#minPriceInput').val(parts[0]);
                $('#maxPriceInput').val(parts[1]);
            } else {
                $('#minPriceInput').val('');
                $('#maxPriceInput').val('');
            }
        }

        // Update only the sidebar tags to show pending selections
        renderFilterTags(true);
    });

    // Intercept Pagination Clicks
    $(document).on('click', '.pagination-link', function (e) {
        e.preventDefault();
        const page = $(this).data('page');
        $('#pageInput').val(page);
        updateProducts();
        window.scrollTo({ top: 0, behavior: 'smooth' });
    });

    // Intercept Sort Links
    $(document).on('click', '.sort-link', function (e) {
        e.preventDefault();
        const sort = $(this).data('sort');
        $('#sortByInput').val(sort);
        $('.text-sort-value').text($(this).find('.text-value-item').text());
        $('.sort-link').removeClass('active');
        $(this).addClass('active');
        updateProducts();
    });


    // Handle Tag Removal
    $(document).on('click', '.remove-tag', function (e) {
        const type = $(this).data('type');
        const val = $(this).data('value');
        const isSidebarTag = $(this).closest('#applied-filters-sidebar').length > 0;

        if (type === 'price') {
            $('#all-price').prop('checked', true);
            $('#minPriceInput').val('');
            $('#maxPriceInput').val('');
        } else if (type === 'caratRange') {
            $('#all-carat').prop('checked', true);
        } else if (type === 'availability') {
            $('#allStock').prop('checked', true);
        } else {
            $(`input[name="${type}"][value="${val}"]`).prop('checked', false);
        }

        if (isSidebarTag) {
            renderFilterTags(true);
        } else {
            filterForm.submit();
        }
    });

    // Clear All Filters
    $(document).on('click', '.btn-clear-all-filters', function () {
        const isSidebarBtn = $(this).closest('#filterShop').length > 0;

        filterForm[0].reset();
        $('#minPriceInput').val('');
        $('#maxPriceInput').val('');
        $('#sortByInput').val('');
        $('#pageInput').val(1);

        if (isSidebarBtn) {
            renderFilterTags(true);
        } else {
            filterForm.submit();
        }
    });

    // Main AJAX Function
    function updateProducts() {
        const formData = filterForm.serialize();
        const url = window.location.pathname + '?' + formData;

        productGrid.addClass('opacity-50');

        $.ajax({
            url: window.location.pathname,
            type: 'GET',
            data: formData,
            headers: { 'X-Requested-With': 'XMLHttpRequest' },
            success: function (html) {
                productGrid.html(html);
                productGrid.removeClass('opacity-50');

                history.pushState(null, '', url);

                // Re-sync tags for both areas after apply
                renderFilterTags(false);

                // Update Total Count
                const newCount = $('.wg-show-more p').text().split('of ')[1]?.split(' ')[0] ||
                    $('.wg-show-more p').text().split('all ')[1]?.split(' ')[0] || '0';
                if (newCount !== '0') {
                    totalCountDisplay.text(newCount);
                } else {
                    totalCountDisplay.text('0');
                }
            },
            error: function () {
                productGrid.removeClass('opacity-50');
            }
        });
    }

    function renderFilterTags(isPending) {
        let tagsHtml = '';

        // Categories
        $('input[name="categoryIds"]:checked').each(function () {
            const label = $(this).next('label').find('span:first').text();
            tagsHtml += `<span class="filter-tag remove-tag" data-type="categoryIds" data-value="${$(this).val()}">${label} <span class="icon icon-close"></span></span>`;
        });

        // Brands
        $('input[name="brandIds"]:checked').each(function () {
            const label = $(this).next('label').find('span:first').text();
            tagsHtml += `<span class="filter-tag remove-tag" data-type="brandIds" data-value="${$(this).val()}">${label} <span class="icon icon-close"></span></span>`;
        });

        // Materials
        $('input[name="goldTypeIds"]:checked').each(function () {
            const label = $(this).next('label').find('span:first').text();
            tagsHtml += `<span class="filter-tag remove-tag" data-type="goldTypeIds" data-value="${$(this).val()}">${label} <span class="icon icon-close"></span></span>`;
        });

        // Product Types
        $('input[name="productTypeIds"]:checked').each(function () {
            const label = $(this).next('label').find('span:first').text();
            tagsHtml += `<span class="filter-tag remove-tag" data-type="productTypeIds" data-value="${$(this).val()}">${label} <span class="icon icon-close"></span></span>`;
        });

        // Diamond Clarities
        $('input[name="diamondClarities"]:checked').each(function () {
            tagsHtml += `<span class="filter-tag remove-tag" data-type="diamondClarities" data-value="${$(this).val()}">${$(this).val()} <span class="icon icon-close"></span></span>`;
        });

        // Diamond Colors
        $('input[name="diamondColors"]:checked').each(function () {
            tagsHtml += `<span class="filter-tag remove-tag" data-type="diamondColors" data-value="${$(this).val()}">${$(this).val()} <span class="icon icon-close"></span></span>`;
        });

        // Diamond Cuts
        $('input[name="diamondCuts"]:checked').each(function () {
            tagsHtml += `<span class="filter-tag remove-tag" data-type="diamondCuts" data-value="${$(this).val()}">${$(this).val()} <span class="icon icon-close"></span></span>`;
        });

        // Carat Range
        const caratVal = $('input[name="caratRange"]:checked').val();
        if (caratVal && caratVal !== "") {
            const label = $('input[name="caratRange"]:checked').next('label').find('span').text();
            tagsHtml += `<span class="filter-tag remove-tag" data-type="caratRange" data-value="${caratVal}">${label} <span class="icon icon-close"></span></span>`;
        }

        // Stone Colors
        $('input[name="stoneColors"]:checked').each(function () {
            tagsHtml += `<span class="filter-tag remove-tag" data-type="stoneColors" data-value="${$(this).val()}">${$(this).val()} <span class="icon icon-close"></span></span>`;
        });

        // Availability (if not "All")
        const avail = $('input[name="availability"]:checked').val();
        if (avail) {
            const label = avail === 'inStock' ? 'In Stock' : 'Out of Stock';
            tagsHtml += `<span class="filter-tag remove-tag" data-type="availability" data-value="${avail}">${label} <span class="icon icon-close"></span></span>`;
        }

        // Price
        const priceVal = $('.price-radio:checked').val();
        if (priceVal && priceVal !== "") {
            const label = $('.price-radio:checked').next('label').find('span').text();
            tagsHtml += `<span class="filter-tag remove-tag" data-type="price" data-value="${priceVal}">${label} <span class="icon icon-close"></span></span>`;
        }

        // Update the requested area
        if (isPending) {
            appliedFiltersSidebar.html(tagsHtml || '<div class="count-text text-main-4">No Filter Selected</div>');
        } else {
            appliedFiltersTop.html(tagsHtml);
            appliedFiltersSidebar.html(tagsHtml || '<div class="count-text text-main-4">No Filter Selected</div>');
        }
    }

    // Ensure sidebar tags are synced whenever it's opened
    document.getElementById('filterShop').addEventListener('show.bs.offcanvas', function () {
        renderFilterTags(true);
    });
});
