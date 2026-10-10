/**
 * Banner Management JavaScript Module
 * Handles AJAX CRUD operations for Banner entity.
 */

(function () {
    var settings = {
        statusFilter: '',
        dataTable: null
    };

    window.BannerManagement = {
        init: function () {
            bindEvents();
            initializeDataTable();
        },

        loadTable: function () {
            loadTableData();
        },

        openModal: function (id) {
            openBannerModal(id);
        },

        save: function (form) {
            var $form = $(form);
            if (!$form.valid()) return;

            var formData = new FormData($form[0]);
            var $btn = $form.find('button[type="submit"]');
            var $spinner = $('#saveSpinner');
            var $text = $('#saveText');

            $btn.prop('disabled', true);
            $spinner.removeClass('d-none');
            $text.text('Saving...');

            $.ajax({
                url: $form.attr('action') || '/Admin/Dashboard/SaveBanner',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (response) {
                    AdminPanel.handleResponse(response, function () {
                        if (typeof response === 'object' && response.success !== false) {
                            $('#bannerModal').modal('hide');
                            loadTableData();
                        } else if (typeof response === 'string' && (response.indexOf('text-danger') > -1 || response.indexOf('form') > -1)) {
                            // Validation errors - replace modal content
                            $('#bannerModalBody').html(response);
                            $.validator.unobtrusive.parse($('#bannerModalBody').find('form'));
                            bindImageUpload();
                        }
                    });
                },
                error: function (xhr) {
                    AdminPanel.showToast('Error: ' + (xhr.responseJSON?.message || xhr.responseText || 'An error occurred'), 'danger');
                },
                complete: function () {
                    $btn.prop('disabled', false);
                    $spinner.addClass('d-none');
                    $text.text('Save Banner');
                }
            });
        },

        delete: function (id) {
            AdminPanel.confirmAction({
                text: "This banner will be permanently deleted!",
                confirmButtonText: 'Yes, delete it!'
            }, function () {
                $.ajax({
                    url: '/Admin/Dashboard/DeleteBanner',
                    type: 'POST',
                    data: {
                        id: id,
                        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function (response) {
                        AdminPanel.handleResponse(response, function () {
                            loadTableData();
                        });
                    },
                    error: function (xhr) {
                        AdminPanel.showToast('Error: ' + (xhr.responseJSON?.message || xhr.responseText || 'An error occurred'), 'error');
                    }
                });
            });
        },

        toggleStatus: function (id) {
            var e = window.event;
            var $btn = $(e.currentTarget || e.target).closest('a');
            var $icon = $btn.find('iconify-icon');

            $.ajax({
                url: '/Admin/Dashboard/ToggleBannerStatus',
                type: 'POST',
                data: {
                    id: id,
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (response) {
                    AdminPanel.handleResponse(response, function () {
                        var $row = $btn.closest('tr');
                        var $badge = $row.find('.status-badge');

                        if (response.isActive) {
                            $icon.attr('icon', 'solar:eye-broken').addClass('text-success').removeClass('text-muted');
                            $badge.removeClass('bg-danger-subtle text-danger')
                                .addClass('bg-success-subtle text-success')
                                .text('Active');
                        } else {
                            $icon.attr('icon', 'solar:eye-closed-broken').addClass('text-muted').removeClass('text-success');
                            $badge.removeClass('bg-success-subtle text-success')
                                .addClass('bg-danger-subtle text-danger')
                                .text('Inactive');
                        }
                    });
                },
                error: function (xhr) {
                    AdminPanel.showToast('Error updating status: ' + (xhr.responseJSON?.message || xhr.responseText || 'An error occurred'), 'danger');
                }
            });
        }
    };

    var bindEvents = function () {
        // Status filter change
        $('.status-option').on('click', function (e) {
            e.preventDefault();
            settings.statusFilter = $(this).data('value');
            $('#statusDropdownBtn').text('Status: ' + $(this).text());
            loadTableData();
        });

        // Add button click
        $('#btnAddBanner').on('click', function () {
            window.BannerManagement.openModal(0);
        });

        // Global Form Submission (Delegated)
        $(document).on('submit', '#bannerModal form', function (e) {
            e.preventDefault();
            window.BannerManagement.save(this);
        });

        // Bind image upload on initial load
        bindImageUpload();
    };

    var bindImageUpload = function () {
        // File Upload Click Handling
        $(document).off('click', '.dropzone').on('click', '.dropzone', function (e) {
            if (e.target.type !== 'file') {
                $(this).find('input[type="file"]').click();
            }
        });

        $(document).off('click', '.dropzone input[type="file"]').on('click', '.dropzone input[type="file"]', function (e) {
            e.stopPropagation();
        });

        $(document).off('change', '.dropzone input[type="file"]').on('change', '.dropzone input[type="file"]', function () {
            previewImage(this);
        });
    };

    var loadTableData = function () {
        var $container = $('#tableContainer');
        $container.html('<div class="text-center py-5"><div class="spinner-border text-primary" role="status"></div><p class="mt-2 text-muted">Loading...</p></div>');

        var url = '/Admin/Dashboard/Banners?isActive=' + settings.statusFilter;

        $.ajax({
            url: url,
            type: 'GET',
            headers: { 'X-Requested-With': 'XMLHttpRequest' },
            success: function (response) {
                $container.html(response);
                initializeDataTable();
            },
            error: function (xhr) {
                $container.html('<div class="text-center py-5 text-danger"><i class="bx bx-error fs-24"></i><p class="mt-2">Error loading banners</p></div>');
            }
        });
    };

    var initializeDataTable = function () {
        if ($.fn.DataTable.isDataTable('#bannerTable')) {
            $('#bannerTable').DataTable().destroy();
        }

        var table = $('#bannerTable').DataTable({
            searching: false,
            lengthChange: false,
            info: false,
            responsive: false,
            dom: "rtp",
            pageLength: 10,
            order: [[4, 'asc']], // Sort by Display Order
            language: {
                emptyTable: "No banners found matching your selection.",
                paginate: {
                    previous: "<i class='bx bx-chevron-left'></i>",
                    next: "<i class='bx bx-chevron-right'></i>"
                }
            },
            initComplete: function () {
                $('#bannerTable').removeClass('dataTable no-footer dtr-inline collapsed');
            },
            drawCallback: function () {
                $('#bannerTable').removeClass('dataTable no-footer dtr-inline collapsed');
                $('.dataTables_empty').addClass('text-center py-4');

                // Hide pagination if only one page exists
                var api = this.api();
                var pages = api.page.info().pages;
                var $paginate = $(api.table().container()).find('.dataTables_paginate');
                if (pages <= 1) {
                    $paginate.hide();
                } else {
                    $paginate.show();
                }
            }
        });
        settings.dataTable = table;
    };

    var openBannerModal = function (id) {
        var $modalBody = $('#bannerModalBody');

        $modalBody.html('<div class="text-center py-5"><div class="spinner-border text-primary" role="status"></div></div>');
        $('#bannerModal').modal('show');

        $.ajax({
            url: '/Admin/Dashboard/GetBannerModal?id=' + id,
            type: 'GET',
            success: function (response) {
                $modalBody.html(response);
                $.validator.unobtrusive.parse($modalBody.find('form'));
                bindImageUpload();
            },
            error: function (xhr) {
                $modalBody.html('<div class="text-center py-5 text-danger"><p>Error loading form</p></div>');
            }
        });
    };

    var previewImage = function (input) {
        if (input.files && input.files[0]) {
            var reader = new FileReader();
            var $dropzone = $(input).closest('.dropzone');
            reader.onload = function (e) {
                $dropzone.find('img').attr('src', e.target.result);
                $dropzone.find('.preview-container').removeClass('d-none');
                $dropzone.find('.upload-icon-group').addClass('d-none');
            }
            reader.readAsDataURL(input.files[0]);
        }
    };

    // Local Toast wrappers removed in favor of AdminPanel centralized handler

})();

$(document).ready(function () {
    if (typeof BannerManagement !== 'undefined') {
        BannerManagement.init();
    }
});
