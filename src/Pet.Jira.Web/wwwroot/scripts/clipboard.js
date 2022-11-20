var clipboard = function () {
    return {
        convertToBlob: function (sourceText, sourceType) {
            return new Blob([sourceText], { type: sourceType });
        }
    }
}