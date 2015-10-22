(function () {
    'use strict';
    var controllerId = 'invites';
    angular.module('app').controller(controllerId, ['$scope', '$http', '$location', '$routeParams', 'common', 'config', 'datacontext', invites]);

    function invites($scope, $http, $location, $routeParams, common, config, datacontext) {
        var vm = this;
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);

        vm.filteredUsers = [];

        vm.title = 'invites';
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
            //TODO: ADD LOGIC TO DELETE
            getUsers(false);
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