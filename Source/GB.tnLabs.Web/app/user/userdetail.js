(function () {
    'use strict';
    var controllerId = 'userdetail';
    angular.module('app').controller(controllerId, ['$scope', '$window', '$routeParams', 'common', 'config', 'datacontext', 'spinner', createUser]);

    function createUser($scope, $window, $routeParams, common, config, datacontext,spinner) {
        var vm = this;
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);

        vm.isNew = true;

        vm.errors = [];
        vm.user = undefined;
        vm.saveChanges = saveChanges;
        vm.back = back;

        activate();

        function activate() {
            onDestroy();
            common.activateController([initUser()], controllerId);
        }

        function back() {
            $window.history.back();
        }

        function initUser() {
            var val = $routeParams.id;
            if (val === 'new') {
                vm.user = datacontext.user.create();
                return;
            }
            vm.isNew = false;
            datacontext.user.getById(val)
                .then(function (data) {
                    vm.user = data;
                }, function (error) {
                    logError('Unable to get participant ' + val);
                    gotoUsers();
                });
        }

        function gotoUsers() {
            $location.path('/participants');
        }

        function saveChanges() {

            spinner.spinnerShow();
            vm.user.password = 'tnLabsP@ss1';
            if (vm.user.entityAspect.validateEntity()) {
                datacontext.saveChanges().then(saveSucceded, saveFailed);
            }
            
            function saveSucceded() {
                spinner.spinnerHide();
                $window.history.back();
            }

            function saveFailed() {
                spinner.spinnerHide();
            }
        }

        function onDestroy() {
            $scope.$on('$destroy', function () {
                datacontext.cancel();
            });
        }



    }
})();