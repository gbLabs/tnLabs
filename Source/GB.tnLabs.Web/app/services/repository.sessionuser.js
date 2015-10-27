(function () {
    'use strict';

    var serviceId = 'repository.sessionuser';

    angular.module('app').factory(serviceId, ['model', 'repository.abstract', repository]);

    function repository(model, AbstractRepository) {
        var entityName = model.entityNames.sessionUser;
        var Predicate = breeze.Predicate;
        var EntityQuery = breeze.EntityQuery;

        function Ctor(mgr) {
            this.serviceId = serviceId;
            this.entityName = entityName;
            this.manager = mgr;
            //Exposed data access functions
            this.getAll = getAll;
            this.create = create;
        }

        AbstractRepository.extend(Ctor);
        return Ctor;

        function create(initialValues) {
            return this.manager.createEntity(entityName, initialValues);
        }

        function getAll(forceRemote, sessionId) {
            var self = this;
            var orderBy = 'identity.email';
            var sessionUsers;
            var predicate = undefined;
            if (sessionId !== undefined) {
                predicate = _sessionPredicate(sessionId);
            }

            if (self._areItemsLoaded() && !forceRemote) {
                sessionUsers = self._getAllLocal(entityName, orderBy, predicate);
                return self.$q.when(sessionUsers);
            }

            return EntityQuery.from('SessionUsers')
                .orderBy(orderBy)
                .where(predicate)
                .expand("identity")
                .toType(entityName)
                .using(self.manager).execute()
                .then(querySucceeded, self._queryFailed);

            function querySucceeded(data) {
                sessionUsers = data.results;
                self._areItemsLoaded(true);
                self.log('Retrieved [Session Participants] from remote data source', sessionUsers.length, true);
                return sessionUsers;
            }

            function _sessionPredicate(filterValue) {
                return Predicate.create('sessionId', '==', filterValue);
            }
        }
      
    }
})();