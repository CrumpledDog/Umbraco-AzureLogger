(function () {
    'use strict';

    angular
        .module('umbraco')
        .directive('autoComplete', AutoCompleteDirective);

    AutoCompleteDirective.$inject = ['$compile'];

    function AutoCompleteDirective($compile) {

        return {
            require: 'ngModel',
            restrict: 'A',
            scope: '=',
            //template: '<ul><li>example autocomplete option</li></ul>',
            link: function (scope, element, attrs, ngModel) {

                // ensure only attached to input element of type text
                if (element[0].tagName.toLowerCase() !== 'input' || element[0].type !== 'text') { return; }

                scope.show = false;

                // build list markup
                var optionsList = angular.element(
                    '<ul ng-show="show" style="position:absolute; left:0; top:' + $(element[0]).height() + 'px; z-index:1; background-color:white; list-style:none; margin:0; border:solid 1px #ccc;">' +
                    '<li style="margin:5px" ng-repeat="option in getData()">{{option}}<li>' +
                    '</ul>');

                $compile(optionsList)(scope);

                element.before(optionsList);

                element.bind('focus', function (event) {
                    scope.show = true;
                    scope.$apply();
                });

                element.bind('blur', function (event) {
                    scope.show = false;
                    scope.$apply();
                });

                //ngModel.$parsers.unshift(function (viewValue) {
                //    if (viewValue === 'foo') {
                //        // revert to previous (model value)
                //        var currentValue = ngModel.$modelValue;
                //        ngModel.$setViewValue(currentValue);
                //        ngModel.$render();
                //        return currentValue;
                //    }
                //    else {
                //        return viewValue;
                //    }
                //});






                // return the expected array
                scope.getData = function () {
                    return scope.$eval(attrs.autoComplete);
                }

            }
        }
    }

})();