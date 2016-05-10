(function () {
    'use strict';

    angular
        .module('umbraco')
        .directive('autoComplete', AutoCompleteDirective);

    AutoCompleteDirective.$inject = ['$compile'];

    function AutoCompleteDirective($compile) {

        return {
            restrict: 'A',
            link: function (scope, element, attrs) {

            }
        }
    }

})();