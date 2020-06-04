$('.modal').on('hidden.bs.modal', function (e) {
    $(this).find('form').trigger('reset');
});