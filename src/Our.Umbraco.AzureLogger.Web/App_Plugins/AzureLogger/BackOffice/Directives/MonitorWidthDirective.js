(function () {
    'use strict';

    angular
        .module('umbraco')
        .directive('monitorWidth', MonitorWidthDirective);

    MonitorWidthDirective.$inject = ['$window', '$parse'];

    function MonitorWidthDirective($window, $parse) {

        return {
            restrict: 'A',
            link: function (scope, element, attrs) {

                angular.element($window).bind('resize', function () {  setWidth(); });

                setWidth(); // startup

                function setWidth() {
                    $parse(attrs.monitorWidth).assign(scope, element[0].offsetWidth);
                }
            }
        }
    }

})();