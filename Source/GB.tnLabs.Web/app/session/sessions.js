(function () {
    'use strict';
    var controllerId = 'sessions';
    angular.module('app').controller(controllerId, ['$routeParams','common','config','datacontext','$location',  sessions]);

    

    function sessions($routeParams, common, config, datacontext, $location) {
        var vm = this;
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);

        vm.setFilter = setFilter;
        vm.filteredSessions = [];
        vm.sessions = [];
        vm.title = 'Sessions';
        vm.removeSession = removeSession;
        activate();

        vm.gotoSession = gotoSession;

        function activate() {
            common.activateController([getSessions()], controllerId)
                .then(function () {
                     log('Activated Sessions View');
                });
        }
        
        function getSessions(forceRefresh) {
            return datacontext.session.getAll(forceRefresh)
                .then(function (data) {
                    vm.sessions = data;
                    applyFilter();
                    return vm.sessions;
                });
        }
        
        function gotoSession(session) {
            if (session && session.sessionId) {
                $location.path('/session/' + session.sessionId);
            }
        }
        
        function applyFilter() {
            vm.filteredSessions = vm.sessions.filter(sessionFilter);
        }

        function sessionFilter(session) {
            if (vm.onlyUpcomingSessions) {
                return !session.isPassed;
            }
            return true;
        }
        
        function setFilter() {
            applyFilter();
        }
        
        function removeSession(session) {
            session.removed = true;
            datacontext.saveChanges();
            getSessions(false);
        }
    }
})();