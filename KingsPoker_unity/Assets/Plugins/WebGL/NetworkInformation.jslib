mergeInto(LibraryManager.library, {
    isNetworkAvailable: function() {
        return navigator.onLine;
    }
});