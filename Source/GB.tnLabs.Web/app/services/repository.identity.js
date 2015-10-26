(function () {
    'use strict';

    var serviceId = 'repository.identity';

    angular.module('app').factory(serviceId, ['model', 'repository.abstract', repository]);

    function repository(model, AbstractRepository) {
        var entityName = model.entityNames.identity;
        var EntityQuery = breeze.EntityQuery;
        var Predicate = breeze.Predicate;
        
        function Ctor(mgr) {
            this.serviceId = serviceId;
            this.entityName = entityName;
            this.manager = mgr;
            //Exposed data access functions
            this.getAll = getAll;
            this.getById = getById;
            this.create = create;
        }

        AbstractRepository.extend(Ctor);
        return Ctor;
        
        function create() {
            return this.manager.createEntity(entityName);
        }

        function getById(id, forceRemote) {
            return this._getById(entityName, id, forceRemote);
        }

        function getAll(forceRemote) {
            var self = this;
            var orderBy = 'email';
            var predicate = 'undefined';
            var identities;

            if (self._areItemsLoaded() && !forceRemote) {
                identities = self._getAllLocal(entityName, orderBy, predicate);
                return self.$q.when(identities);
            }

            return EntityQuery.from('Identities')
                .orderBy(orderBy)
                .toType(entityName)
                .using(self.manager).execute()
                .then(querySucceeded, self._queryFailed);

            function querySucceeded(data) {
                identities = data.results;
                self._areItemsLoaded(true);
                self.log('Retrieved [Identities] from remote data source', identities.length, true);
                return identities;
            }
        }
    }
})();