(function() {
    'use strict';

    angular
        .module('umbraco')
        .filter('highlight', HighlightFilter);

    function HighlightFilter() {

        return function (text, value) {

            if (value) {
                text = text.replace(new RegExp('(' + value + ')', 'gi'), '<span class="highlight">$1</span>');
            }

            return text;
        }

    }

})();