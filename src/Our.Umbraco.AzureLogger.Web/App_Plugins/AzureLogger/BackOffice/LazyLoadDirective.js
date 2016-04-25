angular
    .module('umbraco')
    .directive('lazyLoad', ['$timeout', function ($timeout) {

        return {
            restrict: 'A',
            link: function (scope, element, attrs) {

                // 'locker' to prevent watcher calculating height if it's likely to change
                var expanding = false;

                // returns true if the element doesn't stretch below the bottom of the view
                var elementCanExpand = function () {
                    return (element.offset().top + element.height() < $(window).height() + 500); // 500 = number of pixels below view
                };

                // initialize to ensure content is loaded to fill the initial screen (before any scroll activity)
                var lazyLoad = function () {
                    expanding = true;

                    $timeout(function () { //timeout to ensure scope is ready

                        // TODO: to make more generic, check to see if a promise is actually returned
                        scope.$apply(attrs.lazyLoad) // execute angular expression string
                        .then(function (canLoadMore) {

                            if (canLoadMore && elementCanExpand()) { // check to see if screen filled
                                lazyLoad(); // try again
                            }

                            expanding = false;
                        });
                    });
                };


                // timer to delay event until scrolling stopped
                var delayTimer;

                // jQuery find div with class 'umb-scrollable', as it's this outer element that is scrolled
                $(element).closest('.umb-scrollable').bind('scroll', function () {

                    // cancel any previous timer
                    if (delayTimer) { $timeout.cancel(delayTimer); }

                    // set new timer
                    delayTimer = $timeout(function () {
                        scope.$apply(); // apply scope to trigger watch
                    }, 250);

                });

                // set watch on element height (so it will trigger if data set is reduced)
                scope.$watch(function () {
                    return (!expanding && elementCanExpand());
                }, function (newValue) {
                    if (newValue) {
                        lazyLoad();
                    }
                });

            }
        }
    }]);