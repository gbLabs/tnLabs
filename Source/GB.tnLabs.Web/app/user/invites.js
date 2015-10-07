(function () {
    'use strict';
    var controllerId = 'invites';
    angular.module('app').controller(controllerId, ['$scope', '$http', '$location', '$routeParams', 'common', 'config', 'datacontext', invites]);

    function invites($scope, $http, $location, $routeParams, common, config, datacontext) {
        var vm = this;
        vm.title = 'invites';
        vm.sendInvites = sendInvites;
        activate();

        function activate() {
            common.activateController([], controllerId);
        }

        function sendInvites() {
            datacontext.sendInvites($http, $('#emailsForInvites').val());
        }
    }
})();