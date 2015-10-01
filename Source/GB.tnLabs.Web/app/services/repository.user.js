(function () {
    'use strict';

    var serviceId = 'repository.user';

    angular.module('app').factory(serviceId, ['model', 'repository.abstract', repository]);

    function repository(model, AbstractRepository) {
        var entityName = model.entityNames.user;
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
            var predicate = Predicate.create('removed', '==', 0);
            var users;

            if (self._areItemsLoaded() && !forceRemote) {
                users = self._getAllLocal(entityName, orderBy, predicate);
                return self.$q.when(users);
            }

            return EntityQuery.from('Users')
                .where('removed', '==', '0')
                .orderBy(orderBy)
                .toType(entityName)
                .using(self.manager).execute()
                .then(querySucceeded, self._queryFailed);

            function querySucceeded(data) {
                users = data.results;
                self._areItemsLoaded(true);
                self.log('Retrieved [Users] from remote data source', users.length, true);
                return users;
            }
        }
    }
})();