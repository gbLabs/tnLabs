(function () { 
    'use strict';
    
    var controllerId = 'shell';
    angular.module('app').controller(controllerId,
        ['$rootScope','$scope','$window', 'common', 'config','datacontext', shell]);

    function shell($rootScope, $scope, $window, common, config, datacontext) {
        var vm = this;
        var logSuccess = common.logger.getLogFn(controllerId, 'success');
        var events = config.events;
        vm.busyMessage = 'Please wait ...';
        vm.isBusy = true;
        vm.size = undefined;
        vm.year = moment().year();
        vm.version = undefined;
        
        vm.height = function() {
            return {
                'min-height': vm.minHeight + 'px'
            };
        };
        vm.minHeight = undefined;
        vm.spinnerOptions = {
            radius: 40,
            lines: 7,
            length: 0,
            width: 30,
            speed: 1.7,
            corners: 1.0,
            trail: 100,
            color: '#F58A00'
        };
        activate();

        function activate() {
            logSuccess('Training Labs loaded!', null, true);
            common.activateController([getVersion()], controllerId).then(function () {
                
            });
        }

        function getVersion() {
            return datacontext.getVersion()
               .then(function (data) {
                   return vm.version = data;
               });
        }
        function toggleSpinner(on) {
            vm.isBusy = on;
        }

        $rootScope.$on('$routeChangeStart',
            function(event, next, current) {
                 toggleSpinner(true);
            }
        );
        
        $rootScope.$on('$routeChangeSuccess',
           function (event, next, current) {
               toggleSpinner(false);
           }
       );
        
        $rootScope.$on(events.screenResize,
            function (data, screenResize) {
                vm.size = screenResize.size;
                vm.minHeight =screenResize.minHeight;
            }
        );
        
        $rootScope.$on(events.spinnerToggle,
            function(data, show) {
                toggleSpinner(show);
            }
        );
    };
})();