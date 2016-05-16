(function () {
    'use strict';

    angular
        .module('umbraco')
        .directive('autoComplete', AutoCompleteDirective);

    AutoCompleteDirective.$inject = ['$compile'];

    function AutoCompleteDirective($compile) {

        return {
            //require: 'ngModel',
            restrict: 'A',
            scope: '=',
            //template: '<ul><li>example autocomplete option</li></ul>',
            link: function (scope, element, attrs) {  //, ngModel) {

                // ensure only attached to input element of type text
                if (element[0].tagName.toLowerCase() !== 'input' || element[0].type !== 'text') { return; }

                scope.show = false;
                scope.options = [];
                scope.value = null;

                // build list markup
                var optionsList = angular.element(
                    '<ul class="auto-complete-directive" ng-show="show" style="position:absolute; left:0; top:' + $(element[0]).height() + 'px; z-index:1; background-color:white; list-style:none; margin:0; border:solid 1px #ccc;">' +
                    '<li style="margin:5px" ng-repeat="option in options | filter:value ">{{option}}<li>' +
                    '</ul>');

                // compile list markup with scope
                $compile(optionsList)(scope);

                // add list markup to dom
                element.before(optionsList);

                // show list
                element.bind('focus', function (event) {
                    scope.show = true;
                    scope.$apply();
                });

                // hide list
                element.bind('blur', function (event) {
                    scope.show = false;
                    scope.$apply();
                });


                // http://stackoverflow.com/questions/19167517/angularjs-in-a-directive-that-changes-the-model-value-why-do-i-have-to-call
                // using watch (rather than $parsers) as not changing the value being watched
                scope.$watch(attrs.ngModel, function (value) {
                    scope.value = value;
                });

                // wait until autocomplete data is set (currently set in parent after an ajax call)
                scope.$watch(attrs.autoComplete, function (value) {
                    scope.options = value;
                });
            }
        }
    }

})();