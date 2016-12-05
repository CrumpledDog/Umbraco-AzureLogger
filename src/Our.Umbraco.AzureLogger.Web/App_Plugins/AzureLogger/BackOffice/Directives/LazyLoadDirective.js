(function() {
    'use strict';

    /*
        <lazy-load trigger="method to call" cancel="method to call">
            <div class="umb-pane">
            </div>
        </lazy-load>

        this lazy load directive will keep calling the trigger 'method to call' until the height of <div class="umb-pane">
        fills the screen and while the method returns true (indicating that a retry could return more data)
    */
    angular
        .module('umbraco')
        .directive('lazyLoad', LazyLoadDirective);

    LazyLoadDirective.$inject = ['$timeout'];

    function LazyLoadDirective($timeout) {

        return {
            restrict: 'E',
            template: '<div class="umb-pane" ng-transclude></div>', // using umb-pane for the Umbraco spacing css
            transclude: true,
            link: function (scope, element, attrs) {

                var expanding = false; // locker
                var previousScrollTop = 0;
                var div = $(element[0].firstElementChild); // <div class="umb-pane">

                // jQuery find div with class 'umb-scrollable', as it's this outer element that is scrolled
                $(element).closest('.umb-scrollable').bind('scroll', function () {

                    // calculate direction of scroll
                    var currentScrollTop = $(this).scrollTop();
                    if (currentScrollTop <= previousScrollTop) { // up
                        // TODO: cancel any lazy-load currenty in process

                        //scope.$apply(attrs.cancel);

                    } else { // down
                        if (!expanding && canExpand()) {
                            lazyLoad();
                        }
                    }

                    previousScrollTop = currentScrollTop;
                });

                // HACK: add method to the scope so can be called from controller (we know this is the only instance of the directive being used)
                // the controller will call this after reductive filtering, or a complete clear
                scope.lazyLoad = function () {

                    $timeout(function () { // timeout to ensure scope is ready (and element height calculated correctly)

                        // TODO: cancel any lazy-load currenty in process
                        if (!expanding && canExpand()) { // safety check;
                            lazyLoad();
                        }

                    });
                };

                lazyLoad(); // startup

                // --------------------------------------------------------------------------------

                // returns true if the element doesn't stretch below the bottom of the view
                function canExpand() {
                    return (div.offset().top + div.height() < $(window).height() + 1000); // 1000 = number of pixels below view
                };

                // handles the 'method to call', and attempts to fill the screen
                function lazyLoad() {
                    expanding = true;

                    $timeout(function () { // timeout to ensure scope is ready

                        scope.$apply(attrs.trigger) // execute angular expression string (the 'method to call')
                        .then(function (canLoadMore) { // return value of the promise

                            if (canLoadMore && canExpand()) { // check to see if screen filled
                                lazyLoad(); // try again
                            }

                            expanding = false;
                        });
                    });
                };
            }
        }
    }

})();