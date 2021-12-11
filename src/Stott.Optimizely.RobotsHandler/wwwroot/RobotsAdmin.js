const robotsSuccess = '<div class="alert alert-success alert-dismissible fade show" role="alert">' +
    '<strong>Success!</strong> Your robots.txt content changes were successfully applied.' +
    '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>' +
    '</div>';

const robotsError = '<div class="alert alert-danger alert-dismissible fade show" role="alert">' +
    '<strong>Oh Dear!</strong> An error was encountered when trying to save your robots.txt content.' +
    '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>' +
    '</div>';

$(document).ready(function () {

    var myModal = new bootstrap.Modal(document.getElementById('edit-robots-content-modal'), {
        keyboard: false
    });

    $('.js-edit-button').click(function () {
        let siteId = $(this).data('siteid');
        $.get('/Robots/Details/', { siteId: siteId })
            .done(function (data) {
                $('.js-modal-title').html(data.siteName);
                $('.js-modal-siteid').html(data.siteId);
                $('.js-modal-robots-content').val(data.robotsContent);

                myModal.toggle();
            })
            .fail(function () {
                alert('fail');
            });
    });

    $('.js-save-button').click(function () {
        let siteName = $('.js-modal-title').text();
        let siteId = $('.js-modal-siteid').text();
        let robotsContent = $('.js-modal-robots-content').val();

        $.post('/Robots/Save', { siteId: siteId, siteName: siteName, robotsContent: robotsContent })
            .done(function () {
                $(robotsSuccess).insertAfter('.js-header');
                myModal.toggle();
            })
            .fail(function () {
                $(robotsError).insertAfter('.js-header');
                myModal.toggle();
            });
    });
});