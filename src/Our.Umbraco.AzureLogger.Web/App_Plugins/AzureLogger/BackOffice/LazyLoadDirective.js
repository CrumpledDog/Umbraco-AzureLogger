/*
    <element lazy-load="method to call"></element>

    watches element and scroll activity to check it's filling the screen,
    if not then calls the 'method to call' until its promise returns false
*/
angular
    .module('umbraco')
    .directive('lazyLoad', [
        '$timeout',
        function ($timeout) {

            return {
                restrict: 'A',
                link: function (scope, element, attrs) {

                    // 'locker' to prevent watcher calculating height if it's likely to change
                    var expanding = false;

                    // returns true if the element doesn't stretch below the bottom of the view
                    var elementCanExpand = function () {
                        return (element.offset().top + element.height() < $(window).height() + 500); // 500 = number of pixels below view
                    };

                    // handles the 'method to call'
                    var lazyLoad = function () {
                        expanding = true;

                        $timeout(function () { //timeout to ensure scope is ready

                            // TODO: to make more generic, check to see if a promise is actually returned
                            scope.$apply(attrs.lazyLoad) // execute angular expression string (the 'method to call')
                            .then(function (canLoadMore) { // return value of the promise

                                if (canLoadMore && elementCanExpand()) { // check to see if screen filled
                                    lazyLoad(); // try again
                                }

                                expanding = false;
                            });
                        });
                    };


                    var delayTimer; // timer to delay event until scrolling stopped

                    // jQuery find div with class 'umb-scrollable', as it's this outer element that is scrolled
                    $(element).closest('.umb-scrollable').bind('scroll', function () {

                        if (delayTimer) { $timeout.cancel(delayTimer); } // cancel any previous timer

                        delayTimer = $timeout(function () { // set new timer
                            if (!expanding) { // avoid applying the scope if possible
                                scope.$apply(); // apply scope to trigger watch
                            }
                        }, 250);

                    });

                    // set watch on element (so it will trigger if data set is reduced)
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