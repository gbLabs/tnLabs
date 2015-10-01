(function () {
    'use strict';
    var controllerId = 'users';
    angular.module('app').controller(controllerId, ['$location','$routeParams', 'common', 'config', 'datacontext', users]);

    function users($location,$routeParams, common, config, datacontext) {
        // Using 'Controller As' syntax, so we assign this to the vm variable (for viewmodel).
        var vm = this;
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);

        vm.filteredUsers = [];
        
        vm.title = 'users';
        vm.gotoUser = gotoUser;
        vm.removeUser = removeUser;

        activate();

        function activate() {
            common.activateController([getUsers()], controllerId)
                .then(function () {
                     log('Activated Users View');
                });
        }

        function getUsers(forceRefresh) {
            return datacontext.user.getAll(forceRefresh)
                .then(function(data) {
                    return vm.filteredUsers = data;
                });
        }

        function gotoUser(user) {
            if (user && user.userId) {
                $location.path('/participant/' + user.userId);
            }
        }

        function removeUser(user) {
            user.removed = true;
            datacontext.saveChanges();
            getUsers(false);
        }
    }
})();