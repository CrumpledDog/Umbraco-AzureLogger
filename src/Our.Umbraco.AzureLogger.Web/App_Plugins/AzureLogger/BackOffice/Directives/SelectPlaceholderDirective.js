(function() {
    'use strict';

    angular
        .module('umbraco')
        .directive('placeholder', SelectPlaceholderDirective);

    function SelectPlaceholderDirective() {

        return {
            require: 'ngModel',
            restrict: 'A',
            link: function (scope, element, attrs, ngModel) {

                // ensure only attached to select element
                if (element[0].tagName.toLowerCase() !== 'select') { return; }

                if (attrs.placeholder) {

                    element.prepend('<option value="0" selected="selected" disabled="disabled" style="color:#D9D9D9">' + attrs.placeholder + '</option>');

                    // default to the placeholder option
                    ngModel.$setViewValue('0');

                    // set colour to match placeholder option
                    element.css('color', '#D9D9D9');

                    // reset colour on change
                    element.on('change', function (event) {
                        angular.element(this).css('color', '');
                    });
                }
            }
        }
    }

})();