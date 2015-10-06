(function () {
    'use strict';

    var app = angular.module('app');

    app.directive('resizable', function ($window, common) {
        // resize content area according to window size
        // Usage:
        //  <div data-resizable>
        var directive = {
            link: link,
            restrict: 'A'
        };
        return directive;
        
        function link(scope) {
            var w = angular.element($window);
            scope.getWindowDimensions = function () {
                return {
                    'h': w.height(),
                    'w': w.width()
                };
            };

            scope.$watch(scope.getWindowDimensions, common.resize, true);

            w.bind('resize', function () {
                scope.$apply();
            });

            common.resize({
                'h': w.height(),
                'w': w.width()
            });
        }

    });

    app.directive('ccWidgetClose', function () {
        // Usage:
        // <a data-cc-widget-close></a>
        // Creates:
        // <a data-cc-widget-close="" href="#" class="wclose">
        //     <i class="fa fa-remove"></i>
        // </a>
        var directive = {
            link: link,
            template: '<i class="fa fa-times"></i>',
            restrict: 'A'
        };
        return directive;

        function link(scope, element, attrs) {
            attrs.$set('href', '#');
            attrs.$set('wclose');
            element.click(close);

            function close(e) {
                e.preventDefault();
                element.parent().parent().parent().hide(100);
            }
        }
    });

    app.directive('ccWidgetMinimize', function () {
        // Usage:
        // <a data-cc-widget-minimize></a>
        // Creates:
        // <a data-cc-widget-minimize="" href="#"><i class="fa fa-chevron-up"></i></a>
        var directive = {
            link: link,
            template: '<i class="fa fa-chevron-up"></i>',
            restrict: 'A'
        };
        return directive;

        function link(scope, element, attrs) {
            //$('body').on('click', '.widget .wminimize', minimize);
            attrs.$set('href', '#');
            attrs.$set('wminimize');
            element.click(minimize);

            function minimize(e) {
                e.preventDefault();
                var $wcontent = element.parent().parent().next('.widget-content');
                var iElement = element.children('i');
                if ($wcontent.is(':visible')) {
                    iElement.removeClass('fa fa-chevron-up');
                    iElement.addClass('fa fa-chevron-down');
                    $wcontent.fadeOut(500);
                } else {
                    iElement.removeClass('fa fa-chevron-down');
                    iElement.addClass('fa fa-chevron-up');
                    $wcontent.fadeIn(500);
                }
                
            }
        }
    });

    app.directive('stepScrollToTop', function () {
            // Usage:
            // <wz-step data-step-scroll-to-top></wz-step>
            var directive = {
                link: link,
                restrict: 'A'
            };
            return directive;

            function link(scope, element, attrs) {
                element.find("input[value='Next']").click(function(e) {
                    e.preventDefault();
                    $('body').animate({ scrollTop: 0 }, 500);
                });
                element.find("input[value='Previous']").click(function(e) {
                    e.preventDefault();
                    $('body').animate({ scrollTop: 0 }, 500);
                });
            }
        });

    app.directive('ccScrollToTop', ['$window',
        // Usage:
        // <span data-cc-scroll-to-top></span>
        // Creates:
        // <span data-cc-scroll-to-top="" class="totop">
        //      <a href="#"><i class="fa fa-chevron-up"></i></a>
        // </span>
        function ($window) {
            var directive = {
                link: link,
                template: '<a href="#"><i class="fa fa-chevron-up"></i></a>',
                restrict: 'A'
            };
            return directive;

            function link(scope, element, attrs) {
                var $win = $($window);
                element.addClass('totop');
                $win.scroll(toggleIcon);

                element.find('a').click(function (e) {
                    e.preventDefault();
                    $('body').animate({ scrollTop: 0 }, 500);
                });

                function toggleIcon() {
                    $win.scrollTop() > 300 ? element.slideDown() : element.slideUp();
                }
            }
        }
    ]);

    app.directive('ccSpinner', ['$window', function ($window) {
        // Description:
        //  Creates a new Spinner and sets its options
        // Usage:
        //  <div data-cc-spinner="vm.spinnerOptions"></div>
        var directive = {
            link: link,
            restrict: 'A'
        };
        return directive;

        function link(scope, element, attrs) {
            scope.spinner = null;
            scope.$watch(attrs.ccSpinner, function (options) {
                if (scope.spinner) {
                    scope.spinner.stop();
                }
                scope.spinner = new $window.Spinner(options);
                scope.spinner.spin(element[0]);
            }, true);
        }
    }]);

    app.directive('ccWidgetHeader', function () {
        //Usage:
        //<div data-cc-widget-header title="vm.map.title"></div>
        var directive = {
            link: link,
            scope: {
                'title': '@',
                'subtitle': '@',
                'rightText': '@',
                'allowCollapse': '@'
            },
            templateUrl: '/app/layout/widgetheader.html',
            restrict: 'A',
        };
        return directive;

        function link(scope, element, attrs) {
            attrs.$set('class', 'box-header');
        }
    });
    
    app.directive('datePicker', function () {
        // Usage:
        // <input date-picker></input>
        var directive = {
            link: link,
            restrict: 'A'
        };
        return directive;

        function link(scope, element, attrs) {
            element.datepicker({
                format:'dd MM yyyy',
                startDate: 'today',
                todayBtn: 'linked',
                autoclose: true,
                todayHighlight: true
            });
        }
    });

    app.directive('moDateInput', function ($window) {
        return {
            require: '^ngModel',
            restrict: 'A',
            link: function (scope, elm, attrs, ctrl) {
                var moment = $window.moment;
                var dateFormat = attrs.moDateInput;
                attrs.$observe('moDateInput', function (newValue) {
                    if (dateFormat == newValue || !ctrl.$modelValue) return;
                    dateFormat = newValue;
                    ctrl.$modelValue = new Date(ctrl.$setViewValue);
                });

                ctrl.$formatters.unshift(function (modelValue) {
                    scope = scope;
                    if (!dateFormat || !modelValue) return "";
                    var retVal = moment(modelValue).format(dateFormat);
                    return retVal;
                });

                ctrl.$parsers.unshift(function (viewValue) {
                    scope = scope;
                    var date = moment(viewValue, dateFormat);
                    return (date && date.isValid() && date.year() > 1950) ? date.toDate() : "";
                });
            }
        };
    });

    app.directive('editOnLoad', function ($timeout) {
        return {
            restrict: 'A',
            link: function (scope, element, attrs, ctrl) {
                if (attrs.index == 0 && attrs.email == '') {
                    var run = function () {
                        element.find('button.btn-danger')[0].click();
                    };
                    $timeout(function () {
                        run();
                    }, 10);
                }
            }
        }
    });
    
    app.directive('timePicker', function () {
        // Usage:
        // <input time-picker></input>
        var directive = {
            link: link,
            restrict: 'A'
        };
        return directive;

        function link(scope, element, attrs) {
            element.timepicker({ defaultTime: false, showMeridian: false });
        }
    });
    
    app.directive('ngEnterTab', function () {
        return function (scope, element, attrs) {
            element.bind("keydown keypress", function (event) {
                if (event.which === 13 || event.which === 9) {
                    scope.$apply(function () {
                        scope.$eval(attrs.ngEnterTab);
                    });

                    event.preventDefault();
                }
            });
        };
    });

})();