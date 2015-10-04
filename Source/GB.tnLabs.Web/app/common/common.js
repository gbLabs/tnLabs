(function () {
    'use strict';

    // Define the common module 
    // Contains services:
    //  - common
    //  - logger
    //  - spinner
    var commonModule = angular.module('common', []);

    // Must configure the common service and set its 
    // events via the commonConfigProvider
    commonModule.provider('commonConfig', function () {
        this.config = {
            // These are the properties we need to set
            //controllerActivateSuccessEvent: '',
            //spinnerToggleEvent: ''
        };

        this.$get = function () {
            return {
                config: this.config
            };
        };
    });

    commonModule.factory('common',
        ['$q', '$rootScope', '$timeout', '$window', 'commonConfig', 'logger', common]);

    function common($q, $rootScope, $timeout, $window, commonConfig, logger) {
        var throttles = {};

        var service = {
            // common angular dependencies
            $broadcast: $broadcast,
            $q: $q,
            $timeout: $timeout,
            // generic
            activateController: activateController,
            createSearchThrottle: createSearchThrottle,
            debouncedThrottle: debouncedThrottle,
            isNumber: isNumber,
            logger: logger, // for accessibility
            textContains: textContains,
            resize: resize,
            adjustSize: adjustSize,
            vmCoreNumber: vmCoreNumber,
            vmPricingHour: vmPricingHour
        };

        return service;

        function activateController(promises, controllerId) {
            return $q.all(promises).then(function (eventArgs) {
                var data = { controllerId: controllerId };
                $broadcast(commonConfig.config.controllerActivateSuccessEvent, data);
            });
        }

        function $broadcast() {
            return $rootScope.$broadcast.apply($rootScope, arguments);
        }

        function createSearchThrottle(viewmodel, list, filteredList, filter, delay) {
            // After a delay, search a viewmodel's list using 
            // a filter function, and return a filteredList.

            // custom delay or use default
            delay = +delay || 300;
            // if only vm and list parameters were passed, set others by naming convention 
            if (!filteredList) {
                // assuming list is named sessions, filteredList is filteredSessions
                filteredList = 'filtered' + list[0].toUpperCase() + list.substr(1).toLowerCase(); // string
                // filter function is named sessionFilter
                filter = list + 'Filter'; // function in string form
            }

            // create the filtering function we will call from here
            var filterFn = function () {
                // translates to ...
                // vm.filteredSessions 
                //      = vm.sessions.filter(function(item( { returns vm.sessionFilter (item) } );
                viewmodel[filteredList] = viewmodel[list].filter(function(item) {
                    return viewmodel[filter](item);
                });
            };

            return (function () {
                // Wrapped in outer IFFE so we can use closure 
                // over filterInputTimeout which references the timeout
                var filterInputTimeout;

                // return what becomes the 'applyFilter' function in the controller
                return function(searchNow) {
                    if (filterInputTimeout) {
                        $timeout.cancel(filterInputTimeout);
                        filterInputTimeout = null;
                    }
                    if (searchNow || !delay) {
                        filterFn();
                    } else {
                        filterInputTimeout = $timeout(filterFn, delay);
                    }
                };
            })();
        }

        function debouncedThrottle(key, callback, delay, immediate) {
            // Perform some action (callback) after a delay. 
            // Track the callback by key, so if the same callback 
            // is issued again, restart the delay.

            var defaultDelay = 1000;
            delay = delay || defaultDelay;
            if (throttles[key]) {
                $timeout.cancel(throttles[key]);
                throttles[key] = undefined;
            }
            if (immediate) {
                callback();
            } else {
                throttles[key] = $timeout(callback, delay);
            }
        }

        function isNumber(val) {
            // negative or positive
            return /^[-]?\d+$/.test(val);
        }

        function textContains(text, searchText) {
            return text && -1 !== text.toLowerCase().indexOf(searchText.toLowerCase());
        }

        function adjustSize() {
            var w = angular.element($window);
            resize({
                'h': w.height(),
                'w': w.width()
            });
        }
        
        function resize(newValue) {
            var winHeight = newValue.h;
            var winWidth = newValue.w;
            var screenSize = 'large';
            if (winHeight) {
                $("#content").css("min-height", winHeight);
            }

            if (winWidth < 980 && winWidth > 767) {
                screenSize = 'small';
                if ($(".main-menu-span").hasClass("col-sm-2")) {

                    $(".main-menu-span").removeClass("col-sm-2");
                    $(".main-menu-span").addClass("col-sm-1");

                }

                if ($("#content").hasClass("col-sm-10")) {

                    $("#content").removeClass("col-sm-10");
                    $("#content").addClass("col-sm-11");

                }

            } else {

                if ($(".main-menu-span").hasClass("col-sm-1")) {

                    $(".main-menu-span").removeClass("col-sm-1");
                    $(".main-menu-span").addClass("col-sm-2");

                }

                if ($("#content").hasClass("col-sm-11")) {

                    $("#content").removeClass("col-sm-11");
                    $("#content").addClass("col-sm-10");
                    
                }

            }
            var data = { size: screenSize, minHeight: winHeight };
            $broadcast(commonConfig.config.screenResize, data);
        }
        
        function vmCoreNumber(vmSize) {
            //TODO: this should come from the service so has to be replaced
            switch (vmSize.toLowerCase()) {
                case 'small':
                    return 1;
                case 'medium':
                    return 2;
                case 'large':
                    return 4;
                default:
                    return 1;
            }
        }
        
        function vmPricingHour(vmSize) {
            //TODO: this should come from the service so has to be replaced
            switch (vmSize.toLowerCase()) {
                case 'small':
                    return 0.056;
                case 'medium':
                    return 0.112;
                case 'large':
                    return 0.224;
                default:
                    return 0.056;
            }
        }
    }
})();