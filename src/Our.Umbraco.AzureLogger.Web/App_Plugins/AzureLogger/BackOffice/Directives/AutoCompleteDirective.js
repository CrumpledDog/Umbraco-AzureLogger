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

                scope.cursorIndex = -1;

                // build auto-complete list markup
                var template = angular.element(
                    '<ul class="auto-complete-directive" ng-show="show" style="top:' + $(element[0]).height() + 'px">' +
                        '<li ng-repeat="option in options | filter:value"' +
                            ' ng-class="{cursored: $index == cursorIndex}"' +
                            ' ng-click="selectOption(option)"' +
                            ' ng-bind-html="option | highlight:value">' +
                            //'*{{option}}*' +
                        '</li>' +
                    '</ul>');

                element.before(template); // inject markup
                $compile(template)(scope); // compile markup with scope

                // show list
                element.bind('focus', function (event) {
                    scope.show = true;
                    scope.$apply();
                });

                // hide list
                element.bind('blur', function (event) {
                    scope.show = false;
                    scope.cursorIndex = -1;
                    //scope.$apply(); // when applied, removes the list before it can be clicked
                });

                element.bind('keydown keypress', function (event) {

                    // show on all keys, otherwise hide if tab or enter key
                    scope.show = !(event.which === 9 || event.which === 13);

                    //// handle any cursoring
                    //switch(event.which)
                    //{
                    //    case 9: // tab
                    //    case 13: // enter
                    //        if (scope.cursorIndex > -1) {
                    //            //TODO: get position in filtered collection and set textbox
                    //            scope.cursorIndex = -1; // reset
                    //        }
                    //        break;

                    //    case 38: // cursor up
                    //        if (scope.cursorIndex > -1) {
                    //            scope.cursorIndex--;
                    //        }
                    //        event.preventDefault();
                    //        break;

                    //    case 40: // cursor down
                    //        if (scope.cursorIndex < 7) { // TODO: get length of filterd collection
                    //            scope.cursorIndex++;
                    //        }
                    //        event.preventDefault();
                    //        break;

                    //    default:
                    //        scope.cursorIndex = -1;
                    //        break;
                    //}

                    scope.$apply();
                });

                // watch the input text - using watch (rather than $parsers) as not changing the value being watched
                // http://stackoverflow.com/questions/19167517/angularjs-in-a-directive-that-changes-the-model-value-why-do-i-have-to-call
                scope.$watch(attrs.ngModel, function (value) {
                    scope.value = value;
                });

                // wait until autocomplete data is set (currently set in parent after an ajax call)
                scope.$watch(attrs.autoComplete, function (value) {
                    scope.options = value;
                    // TODO: destroy this wach once value has been set ?
                });

                // option clicked, set textbox value
                scope.selectOption = function (option) {
                    ngModel.$setViewValue(option);
                    ngModel.$render();
                };
            }
        }
    }

})();