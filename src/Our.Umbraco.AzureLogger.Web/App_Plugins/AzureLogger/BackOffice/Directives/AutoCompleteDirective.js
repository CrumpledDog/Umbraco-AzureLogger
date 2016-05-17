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
            link: function (scope, element, attrs, ngModel) {

                // ensure only attached to input element of type text
                if (element[0].tagName.toLowerCase() !== 'input' || element[0].type !== 'text') { return; }

                var template = angular.element(
                    '<ul class="auto-complete-directive" ng-show="show" style="top:' + $(element[0]).height() + 'px">' +
                    '<li ng-repeat="option in options | filter:value" ng-click="selectOption(option)">{{option}}</li>' +
                    '</ul>');

                element.before(template);
                $compile(template)(scope);

                // show list
                element.bind('focus', function (event) {
                    scope.show = true;
                    scope.$apply();
                });

                // hide list
                element.bind('blur', function (event) {
                    scope.show = false;
                    //scope.$apply(); // when applied, removes the list before it can be clicked
                });

                element.bind('keydown keypress', function (event) {
                    if (event.which === 9) { // if tab key
                        scope.show = false;
                        scope.$apply();
                    }
                });

                // http://stackoverflow.com/questions/19167517/angularjs-in-a-directive-that-changes-the-model-value-why-do-i-have-to-call
                // using watch (rather than $parsers) as not changing the value being watched
                scope.$watch(attrs.ngModel, function (value) {
                    console.log(value);
                    scope.value = value;
                });

                // wait until autocomplete data is set (currently set in parent after an ajax call)
                scope.$watch(attrs.autoComplete, function (value) {
                    scope.options = value;
                });

                scope.selectOption = function (option) {
                    ngModel.$setViewValue(option);
                    ngModel.$render();
                };
            }
        }
    }

})();