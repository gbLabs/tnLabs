(function () {
    'use strict';

    var serviceId = 'repository.abstract';

    angular.module('app').factory(serviceId, ['common','config', AbstractRepositry]);

    function AbstractRepositry(common, config) {
        var Predicate = breeze.Predicate;
        var EntityQuery = breeze.EntityQuery;
        var $q = common.$q;

        //Abstract repo gets its derived object's this.manager
        function Ctor() {
            this.isLoaded = false;
        }

        Ctor.extend = function(repoCtor) {
            repoCtor.prototype = new Ctor();
            repoCtor.prototype.constructor = repoCtor;
        };
        //Shared by repositories classes
        Ctor.prototype._areItemsLoaded = _areItemsLoaded;
        Ctor.prototype._getAllLocal = _getAllLocal;
        Ctor.prototype._getAllLocalWithTake = _getAllLocalWithTake;
        Ctor.prototype._getById = _getById;
        Ctor.prototype._getFirstByDate = _getFirstByDate;
        Ctor.prototype._getInlineCount = _getInlineCount;
        Ctor.prototype._getLocalEntityCount = _getLocalEntityCount;
        Ctor.prototype._getLocalFirstBy = _getLocalFirstBy;
        Ctor.prototype._queryFailed = _queryFailed;
        //functions for the repo
        Ctor.prototype.log = common.logger.getLogFn(this.serviceId);
        
        Ctor.prototype.$q = common.$q;
        return Ctor;

        function _areItemsLoaded(value) {
            if (value === undefined) {
                return this.isLoaded; // get
            }
            return this.isLoaded = value; // set
        }

        function _getAllLocal(resource, ordering, predicate) {
            return EntityQuery.from(resource)
                .orderBy(ordering)
                .where(predicate)
                .using(this.manager)
                .executeLocally();
        }
        
        function _getAllLocalWithTake(resource, ordering, predicate,take) {
            return EntityQuery.from(resource)
                .orderBy(ordering)
                .take(take)
                .where(predicate)
                .using(this.manager)
                .executeLocally();
        }
        
        function _getById(entityName, id, forceRemote) {
            var self = this;
            var manager = self.manager;
            if (!forceRemote) {
                // check cache first
                var entity = manager.getEntityByKey(entityName, id);
                if (entity && !entity.isPartial) {
                    self.log('Retrieved [' + entityName + '] id:' + id + ' from cache.', entity, true);
                    if (entity.entityAspect.entityState.isDeleted()) {
                        entity = null; // hide session marked-for-delete
                    }
                    return $q.when(entity);
                }
            }

            // Hit the server
            // It was not found in cache, so let's query for it.
            return manager.fetchEntityByKey(entityName, id)
                .then(querySucceeded, self._queryFailed);

            function querySucceeded(data) {
                entity = data.entity;
                if (!entity) {
                    self.log('Could not find [' + entityName + '] id:' + id, null, true);
                    return null;
                }
              
                self.log('Retrieved [' + entityName + '] id ' + entity.id
                    + ' from remote data source', entity, true);
          
                return entity;
            }
        }
        
        function _getFirstByDate(entityName,orderBy, predicate) {
            if (this._areItemsLoaded()) {
                return $q.when(this._getLocalFirstBy(entityName,orderBy));
            }

            return EntityQuery.from(entityName).orderBy(orderBy).take(1)
               .using(manager).execute()
               .then(querySucceeded);

            function querySucceeded(data) {
                var entity = data.results;
                return entity;
            }
        }
        
        function _getInlineCount(data) { return data.inlineCount; }
        
        function _getLocalEntityCount(resource, predicate) {
            var entities = EntityQuery.from(resource)
                .where(predicate)
                .using(this.manager)
                .executeLocally();
            return entities.length;
        }
        
        function _getLocalFirstBy(entityName,orderBy,predicate) {
            var entities = EntityQuery.from(entityName)
                .where(predicate)
                .orderBy(orderBy).take(1)
                .using(this.manager)
                .executeLocally();
            return entities;
        }

        function _queryFailed(error) {
            //var msg = config.appErrorPrefix + 'Error retrieving data.' + error.message;
            //logError(msg, error);
            throw error;
        }
    }
})();