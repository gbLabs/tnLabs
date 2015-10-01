(function () {
    'use strict';

    var serviceId = 'repositories';

    angular.module('app').factory(serviceId, ['$injector', repositories]);

    function repositories($injector) {
        var manager;
        var service = {
            getRepo: getRepo,
            init:init
        };

        return service;

        //called exclusively by datacontext
        function init(mgr) { manager = mgr; }

        function getRepo(repoName) {
            var fullRepoName = 'repository.' + repoName.toLowerCase();
            var Repo = $injector.get(fullRepoName);
            return new Repo(manager);

        }
    }
})();