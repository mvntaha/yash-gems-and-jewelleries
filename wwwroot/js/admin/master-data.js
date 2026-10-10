/**
 * Master Data Management JavaScript Module
 * Handles AJAX CRUD operations for all master entities.
 */

(function () {
    var settings = {
        entityType: '',
        statusFilter: '',
        dataTable: null
    };

    window.MasterData = {
        init: function () {
            bindEvents();
            loadInitialState();
        },

        loadTable: function (type) {
            settings.entityType = type;
            updateUIForEntity();
            loadTableData();
        },

        openModal: function (id) {
            openMasterModal(id);
        },

        save: function (form) {
            var $form = $(form);
            if (!$form.valid()) return;

            var formData = new FormData($form[0]);
            var $btn = $form.find('button[type="submit"]');
            var originalText = $btn.text();

            $btn.prop('disabled', true).text('Saving...');

            $.ajax({
                url: $form.attr('action') || '/Admin/Master/Save',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (response) {
                    AdminPanel.handleResponse(response, function () {
                        if (typeof response === 'object' && response.success !== false) {
                            $('#masterModal').modal('hide');
                            loadTableData();
                        } else if (typeof response === 'string' && (response.indexOf('text-danger') > -1 || response.indexOf('form') > -1)) {
                            $('#masterModalBody').html(response);
                            $.validator.unobtrusive.parse($('#masterModalBody').find('form'));
                        }
                    });
                },
                error: function (xhr) {
                    AdminPanel.showToast('Error: ' + xhr.responseText, 'danger');
                },
                complete: function () {
                    $btn.prop('disabled', false).text(originalText);
                }
            });
        },

        delete: function (id) {
            AdminPanel.confirmAction({
                text: "This action cannot be undone!",
                confirmButtonText: 'Yes, delete it!'
            }, function () {
                $.ajax({
                    url: '/Admin/Master/Delete',
                    type: 'POST',
                    data: {
                        entityType: settings.entityType,
                        id: id,
                        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function (response) {
                        AdminPanel.handleResponse(response, function () {
                            loadTableData();
                        });
                    },
                    error: function (xhr) {
                        AdminPanel.showToast('Error: ' + xhr.responseText, 'error');
                    }
                });
            });
        },

        toggleStatus: function (id, entityType) {
            var e = window.event;
            var $btn = $(e.currentTarget || e.target).closest('a');
            var $icon = $btn.find('iconify-icon');

            $.ajax({
                url: '/Admin/Master/ToggleStatus',
                type: 'POST',
                data: {
                    id: id,
                    entityType: entityType,
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
                    AdminPanel.showToast('Error updating status: ' + xhr.responseText, 'danger');
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
            if (settings.entityType) loadTableData();
        });

        // Add button click
        $('#btnAddMaster').on('click', function () {
            window.MasterData.openModal(0);
        });

        // Global Form Submission (Delegated)
        $(document).on('submit', '#masterModal form', function (e) {
            e.preventDefault();
            window.MasterData.save(this);
        });

        // File Upload Click Handling
        $(document).on('click', '.dropzone', function (e) {
            if (e.target.type !== 'file') {
                $(this).find('input[type="file"]').click();
            }
        });

        $(document).on('click', '.dropzone input[type="file"]', function (e) {
            e.stopPropagation();
        });

        $(document).on('change', '.dropzone input[type="file"]', function () {
            previewImage(this);
        });
    };

    var loadInitialState = function () {
        var urlParams = new URLSearchParams(window.location.search);
        var entity = urlParams.get('type');
        if (entity) {
            settings.entityType = entity;
            updateUIForEntity();
            loadTableData();
        }
    };

    var updateUIForEntity = function () {
        var $btn = $('#btnAddMaster');
        var $title = $('#masterTitle');
        if (settings.entityType) {
            $btn.prop('disabled', false);
            var displayName = settings.entityType.charAt(0).toUpperCase() + settings.entityType.slice(1);
            if (displayName.endsWith('s')) displayName = displayName.slice(0, -1);
            $btn.text('Add ' + displayName);
            $title.text(displayName + ' Management');
        } else {
            $btn.prop('disabled', true);
            $btn.text('Add');
            $title.text('Master Data Management');
        }
    };

    var loadTableData = function () {
        var $container = $('#tableContainer');
        $container.html('<div class="text-center py-5"><div class="spinner-border text-primary" role="status"></div><p class="mt-2 text-muted">Loading...</p></div>');
        var url = '/Admin/Master/List?entityType=' + settings.entityType + '&isActive=' + settings.statusFilter;
        $.ajax({
            url: url,
            type: 'GET',
            headers: { 'X-Requested-With': 'XMLHttpRequest' },
            success: function (response) {
                $container.html(response);
                initializeDataTable();
            }
        });
    };

    var initializeDataTable = function () {
        if ($.fn.DataTable.isDataTable('#masterTable')) {
            $('#masterTable').DataTable().destroy();
        }

        var table = $('#masterTable').DataTable({
            searching: false,
            lengthChange: false,
            info: false,
            responsive: false,
            dom: "rtp",
            pageLength: 10,
            language: {
                emptyTable: "No records found matching your selection.",
                paginate: {
                    previous: "<i class='bx bx-chevron-left'></i>",
                    next: "<i class='bx bx-chevron-right'></i>"
                }
            },
            initComplete: function () {
                $('#masterTable').removeClass('dataTable no-footer dtr-inline collapsed');
            },
            drawCallback: function () {
                $('#masterTable').removeClass('dataTable no-footer dtr-inline collapsed');
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

    var openMasterModal = function (id) {
        var $modalBody = $('#masterModalBody');
        var entity = settings.entityType;
        if (!entity) return;

        $modalBody.html('<div class="text-center py-5"><div class="spinner-border text-primary" role="status"></div></div>');
        $('#masterModal').modal('show');
        $.ajax({
            url: '/Admin/Master/GetModal?entityType=' + entity + '&id=' + id,
            type: 'GET',
            success: function (response) {
                $modalBody.html(response);
                $.validator.unobtrusive.parse($modalBody.find('form'));
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

    // Local Toast wrappers removed in favor of AdminToast centralized handler

})();

$(document).ready(function () {
    window.MasterData.init();
});
