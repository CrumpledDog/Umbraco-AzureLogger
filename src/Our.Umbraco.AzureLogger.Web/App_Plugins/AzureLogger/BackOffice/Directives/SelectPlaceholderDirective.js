(function() {
    'use strict';

    angular
        .module('umbraco')
        .directive('placeholder', [SelectPlaceholderDirective]);

    function SelectPlaceholderDirective() {

        return {
            require: 'ngModel',
            restrict: 'A',
            link: function (scope, element, attrs, ngModel) {

                // ensure only attached to select element
                if (element[0].tagName.toLowerCase() !== 'select') {
                    return;
                }

                if (attrs.placeholder) {

                    // TODO: move styling from CSS to here
                    //for (var i = 0; i < element.children.length; i++) {
                    //    angular.element(element.children[i]).css('color', 'black');
                    //}

                    //element.prepend('<option value="-1" selected="selected" style="color:#D9D9D9">' + attrs.placeholder + '</option>');
                    element.prepend('<option value="-1" selected="selected" disabled="disabled" style="color:#D9D9D9">' + attrs.placeholder + '</option>');

                    // default to the placeholder option
                    ngModel.$setViewValue(-1);

                    // set colour to match placeholder option
                    element.css('color', '#D9D9D9');

                    // update colour on any change
                    element.on('change', function (event) {
                        if (event.target.value == -1) {
                            angular.element(this).css('color', '#D9D9D9');
                        } else {
                            angular.element(this).css('color', 'black');
                        }
                    });
                }
            }
        }
    }

})();