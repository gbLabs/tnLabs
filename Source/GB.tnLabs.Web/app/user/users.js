(function () {
    'use strict';
    var controllerId = 'users';
    angular.module('app').controller(controllerId, ['$scope', '$http', '$location', '$routeParams', 'common', 'config', 'datacontext', users]);

    function users($scope, $http, $location, $routeParams, common, config, datacontext) {
        var vm = this;
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);

        vm.filteredUsers = [];

        vm.title = 'users';
        vm.sendInvites = sendInvites;
        vm.gotoUser = gotoUser;
        vm.removeUser = removeUser

        activate();

        function activate() {
            common.activateController([getUsers()], controllerId).then(function () {
                log('Activated Users View');
            });
        }

        function sendInvites() {
            datacontext.sendInvites($http, $('#emailsForInvites').val());
        }

        function getUsers(forceRefresh) {
            return datacontext.identity.getAll(forceRefresh)
                .then(function (data) {
                    return vm.filteredUsers = data;
                });
        }

        function gotoUser(user) {
            if (user && user.identityId) {
                $location.path('/participant/' + user.identityId);
            }
        }

        function removeUser(user) {
            if (user && user.identityId) {
                datacontext.removeSubscriptionUser($http, user.identityId);
            }
            getUsers(true);
        }
    }
})();

function onInvitesSent(result) {
    $('#on-invites-sent').empty();
    if ($('#on-invites-sent').hasClass('hidden')) {
        $('#on-invites-sent').removeClass('hidden');
    }
    $('#on-invites-sent').append('<strong>Info</strong> Number of invites sent: ' + result);
}