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

                        ngModel.$setViewValue(-1);

                        element.css('color', '#aaa');

                        element.on('change', function (event) {
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