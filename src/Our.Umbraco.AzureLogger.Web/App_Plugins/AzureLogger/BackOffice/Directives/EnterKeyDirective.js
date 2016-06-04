(function() {
    'use strict';

    angular
        .module('umbraco')
        .directive('enterKey', EnterKeyDirective);

    function EnterKeyDirective() {

        return {
            restrict: 'A',
            link: function (scope, element, attrs) {

                element.bind('keydown keypress', function (event) {
                    if (event.which === 13) { // if enter key

                        // call the enterKey func and expect promise
                        scope.$apply(attrs.enterKey)
                        .then(function () {

                            element[0].disabled = false; // can't wait for ng-disabled, as that's outside enterKey promise
                            element[0].focus();

                        });

                        event.preventDefault();
                    }
                });

            }
        }
    }

})();