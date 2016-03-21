angular
    .module('umbraco')
    .directive('lazyLoad', ['$timeout', function ($timeout) {

        return {
            restrict: 'A',
            link: function (scope, element, attrs) {

                // returns true if the element hasn't overlaped the bottom of the view
                var lazyLoad = function () {
                    var overlap = 500;

                    console.log('scrolly');
                    console.log(element.offset().top);
                    console.log(element.height());
                    console.log($(window).height());

                    return (element.offset().top + element.height() < $(window).height() + overlap);
                };

                // initialize to ensure content is loaded to fill the initial screen (before any scroll activity)
                $timeout(function () {
                    //TODO: check the lazy load has filled the screen
                    if (lazyLoad()) {
                        scope.$apply(attrs.lazyLoad);
                    }
                });

                var scrollDelayTimer;
                // jQuery walk up tree from element to find div with class 'umb-scrollable', to hook into when this is scrolled
                $(element).closest('.umb-scrollable').bind('scroll', function () {

                    // cancel any previous timer and set new one
                    if (scrollDelayTimer) {
                        $timeout.cancel(scrollDelayTimer);
                    }

                    scrollDelayTimer = $timeout(function () {
                        if (lazyLoad()) {
                            scope.$apply(attrs.lazyLoad);
                        }
                    }, 250);

                });
            }
        }
    }]);