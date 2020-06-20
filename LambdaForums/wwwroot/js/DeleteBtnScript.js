function confirmDelete(forumId, isDeleteClicked) {
    var deleteSpan = 'deleteSpan_' + forumId;
    var confirmDeleteSpan = 'confirmDeleteSpan_' + forumId;

    if (isDeleteClicked) {
        $('#' + deleteSpan).hide();
        $('#' + confirmDeleteSpan).show();
    } else {
        $('#' + deleteSpan).show();
        $('#' + confirmDeleteSpan).hide();
    }

}