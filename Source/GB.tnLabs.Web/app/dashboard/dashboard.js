(function () {
    'use strict';
    var controllerId = 'dashboard';
    angular.module('app').controller(controllerId, ['$location','common', 'datacontext', dashboard]);

    function dashboard($location,common, datacontext) {
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);

        var vm = this;
        vm.news = {
            title: 'Labs',
            description: 'Training labs.'
        };

        vm.gotoLab = gotoLab;
        vm.gotoSession = gotoSession;
        vm.labsCount = 0;
        vm.sessionsCount = 0;
        vm.sessions = [];
        vm.labs = [];
        vm.nextSession = undefined;
        vm.title = 'Labs';

        activate();

        function activate() {
            var promises = [getLabsCount(), getSessions(), getSessionsCount(), getLabs(), getNextSession()];
            common.activateController(promises, controllerId)
                .then(function () { log('Activated Dashboard View'); });
        }

        function getNextSession() {
        	return datacontext.session.getFirstByDate().then(function (data) {
        	        vm.nextSession = data[0];
        	        if (vm.nextSession !== undefined) {
        	            vm.nextSession.entityAspect.loadNavigationProperty("sessionUsers");
        	        }
                return vm.nextSession;
            });
        }

        function getLabsCount() {
            return datacontext.lab.getCount().then(function (data) {
                return vm.labsCount = data;
            });
        }

        function getSessionsCount() {
            return datacontext.session.getCount()
                .then(function (data) {
                    return vm.sessionsCount = data;
                });
        }
        
        function getSessions() {
            return vm.sessions = datacontext.session.getUpcomingThreeSessions();
        }
        
        function getLabs() {
            return vm.labs = datacontext.lab.getLatestThreeLabs();
        }

        function gotoLab(lab) {
            if (lab && lab.labId) {
                $location.path('/lab/' + lab.labId);
            }
        }
        function gotoSession(session) {
            if (session && session.sessionId) {
                $location.path('/session/' + session.sessionId);
            }
        }
    }
})();