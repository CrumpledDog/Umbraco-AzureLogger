(function() {
    'use strict';

    /*
        <lazy-load trigger="method to trigger" abort="method to abort">
            <div class="umb-pane">

            </div>
        </lazyLoad>

        this lazy load directive will keep calling the 'method to trigger' until the height of <div class="umb-pane">
        fills the screen and while the method returns true (indicating that a retry could return more data)
    */
    angular
        .module('umbraco')
        .directive('lazyLoad', LazyLoadDirective);

    LazyLoadDirective.$inject = ['$timeout'];

    function LazyLoadDirective($timeout) {

        return {
            restrict: 'E',
            template: '<div class="umb-pane" ng-transclude></div>',
            transclude: true,
            link: function (scope, element, attrs) {

                var expanding = false; // locker
                var previousScrollTop = 0;

                // jQuery find div with class 'umb-scrollable', as it's this outer element that is scrolled
                $(element).closest('.umb-scrollable').bind('scroll', function () {

                    // calculate direction of scroll
                    var currentScrollTop = $(this).scrollTop();
                    if (currentScrollTop <= previousScrollTop) { // up
                        // TODO: cancel any lazy-load currenty in process
                    } else { // down
                        if (!expanding && elementCanExpand()) {
                            lazyLoad();
                        }
                    }

                    previousScrollTop = currentScrollTop;
                });

                // HACK: add method to the scope so can be called from controller (we know this is the only instance of the directive being used)
                // the controller will call this after reductive filtering, or a complete clear
                scope.lazyLoad = function () {
                    // TODO: cancel any lazy-load currenty in process
                    if (!expanding && elementCanExpand()) { // safety check;
                        lazyLoad();
                    }
                };

                lazyLoad(); // startup

                // --------------------------------------------------------------------------------

                // returns true if the element doesn't stretch below the bottom of the view
                function elementCanExpand() {

                    var div = $(element[0].firstElementChild); // <div class="umb-pane">

                    return (div.offset().top + div.height() < $(window).height() + 1000); // 1000 = number of pixels below view
                }

                // handles the 'method to call', and attempts to fill the screen
                function lazyLoad() {
                    expanding = true;

                    $timeout(function () { // timeout to ensure scope is ready

                        scope.$apply(attrs.trigger) // execute the 'method to call'
                        .then(function (canLoadMore) { // return value of the promise

                            if (canLoadMore && elementCanExpand()) { // check to see if screen filled
                                lazyLoad(); // try again
                            }
                            else {
                                expanding = false;
                            }

                        });
                    });
                }
            }
        }
    }

})();