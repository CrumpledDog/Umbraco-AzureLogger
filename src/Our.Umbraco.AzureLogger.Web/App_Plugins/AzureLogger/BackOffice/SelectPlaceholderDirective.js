angular
    .module('umbraco')
    .directive('placeholder', [
        function () {

            return {
                require: 'ngModel',
                restrict: 'A',
                link: function (scope, element, attrs, ngModel) {

                    // ensure only attached to select element
                    if (element[0].tagName.toLowerCase() !== 'select') {
                        return;
                    }

                    if (attrs.placeholder) {

                        //for (var i = 0; i < element.children.length; i++) {
                        //    angular.element(element.children[i]).css('color', 'black');
                        //}

                        element.prepend('<option value="-1" selected="selected" style="color:#aaa">' + attrs.placeholder + '</option>');
                        //element.prepend('<option value="-1" selected="selected" style="display:none" hidden="hidden">' + attrs.placeholder + '</option>');
                        //element.prepend('<option value="-2"></option>');

                        // default to the placeholder option
                        ngModel.$setViewValue(-1);

                        // set colour to match placeholder option
                        element.css('color', '#aaa');

                        // update colour on any change
                        element.on('change', function (event) {

                            //if (event.target.value == -2) {
                            //    ngModel.$setViewValue(-1);
                            //    event.target.value = -1;
                            //}
                            if (event.target.value == -1) {
                                angular.element(this).css('color', '#aaa');
                            } else {
                                angular.element(this).css('color', 'black');
                            }
                        });
                    }
                }
            }
        }]);