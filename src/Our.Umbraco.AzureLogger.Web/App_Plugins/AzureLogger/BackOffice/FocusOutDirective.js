/*
    alternative for ng-blur, as that isn't available in the verion of angular being used
*/
angular
    .module('umbraco')
    .directive('focusOut', [
        function () {

            return {
                restrict: 'A',
                link: function (scope, element, attrs) {

                    element.bind('blur', function (event) {
                        scope.$apply(function () {
                            scope.$eval(attrs.focusOut, { $event: event });
                        });
                    });

                }
            }
        }]);