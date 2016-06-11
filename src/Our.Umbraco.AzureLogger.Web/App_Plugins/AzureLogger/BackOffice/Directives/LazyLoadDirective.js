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
                var previousScrollTop = 0; // calculate scroll direction
                var div = $(element[0].firstElementChild); // <div class="umb-pane">

                scope.lazyLoad = lazyLoad; // the controller will call this after reductive filtering, or a complete clear

                // scrolling
                $(element).closest('.umb-scrollable').bind('scroll', function () {

                    var currentScrollTop = $(this).scrollTop();
                    if (currentScrollTop <= previousScrollTop) {
                        // up
                        scope.$apply(attrs.abort);
                    } else {
                        // down
                        lazyLoad();
                    }

                    previousScrollTop = currentScrollTop;
                });

                lazyLoad(); // startup

                // --------------------------------------------------------------------------------

                // returns true if the div doesn't stretch below the bottom of the view
                function canExpand() {
                    return (div.offset().top + div.height() < $(window).height() + 1000); // 1000 = number of pixels below view
                }

                // activates the trigger
                function lazyLoad() {

                    // TODO: cancel any trigger currenty in process

                    if (!expanding && canExpand()) { // safety check;
                        expanding = true;
                        trigger();
                    }
                }

                function trigger() {

                    $timeout(function () { // timeout to ensure scope is ready

                        scope.$apply(attrs.trigger) // execute the 'method to call'
                        .then(function (tryAgain) { // return value of the promise

                            if (tryAgain && canExpand()) { // check to see if screen filled
                                trigger();
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