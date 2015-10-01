(function () {
    'use strict';

    var serviceId = 'repository.lab';

    angular.module('app').factory(serviceId, ['model','repository.abstract', RepositoryLab]);

    function RepositoryLab(model, AbstractRepository) {
        var entityName = model.entityNames.lab;
        var Predicate = breeze.Predicate;
        var EntityQuery = breeze.EntityQuery;
        
        function Ctor(mgr) {
            this.serviceId = serviceId;
            this.entityName = entityName;
            this.manager = mgr;
            //Exposed data access functions
            this.create = create;
            this.getAll = getAll;
            this.getById = getById;
            this.getCount = getCount;
            this.getLatestThreeLabs = getLatestThreeLabs;
        }

        AbstractRepository.extend(Ctor);
        return Ctor;
        
        function create() {
            return this.manager.createEntity(entityName);
        }
        
        function getAll(forceRemote) {
            var self = this;
            var orderBy = 'creationDate desc';
            var predicate = Predicate.create('removed', '==', 0); 
            var labs;

            if (self._areItemsLoaded() && !forceRemote) {
                labs = self._getAllLocal(entityName, orderBy, predicate);
                return self.$q.when(labs);
            }

            return EntityQuery.from('Labs')
                .select('labId, name, imageName, description, creationDate, removed')
                .where('removed','==','0')
                .orderBy(orderBy)
                .toType(entityName)
                .using(self.manager).execute()
                .then(querySucceeded, self._queryFailed);

            function querySucceeded(data) {
                labs = data.results;
                self._areItemsLoaded(true);
                self.log('Retrieved [Labs] from remote data source', labs.length, true);
                return labs;
            }
        }
        
        function getLatestThreeLabs() {
            var self = this;
            var orderBy = 'creationDate desc';
            
            return self._getAllLocalWithTake(entityName, orderBy, undefined, 3);
        }

        function getById(id, forceRemote) {
            return this._getById(entityName, id, forceRemote);
        }
        
        function getCount() {
            var self = this;
            if (self._areItemsLoaded()) {
                return self.$q.when(self._getLocalEntityCount(entityName));
            }

            return EntityQuery.from('Labs').take(0).inlineCount()
                .using(self.manager).execute()
                .then(self._getInlineCount);
        }
    }
})();