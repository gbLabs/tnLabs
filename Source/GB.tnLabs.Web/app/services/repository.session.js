(function () {
    'use strict';

    var serviceId = 'repository.session';

    angular.module('app').factory(serviceId, ['model', 'repository.abstract', repository]);

    function repository(model, AbstractRepository) {
        var entityName = model.entityNames.session;
        var EntityQuery = breeze.EntityQuery;
        var Predicate = breeze.Predicate;
        var self = undefined;


        function Ctor(mgr) {
            this.serviceId = serviceId;
            this.entityName = entityName;
            this.manager = mgr;
            //Exposed data access functions
            this.create = create;
            this.getAll = getAll;
            this.getById = getById;
            this.getCount = getCount;
            this.getFirstByDate = getFirstByDate;
            this.getParticipant = getParticipant;
            this.getUpcomingThreeSessions = getUpcomingThreeSessions;
            this.isParticipantInSession = isParticipantInSession;
            this.removeParticipant = removeParticipant;
        }

        AbstractRepository.extend(Ctor);
        return Ctor;

        function _notRemovedPredicate() {
            return  Predicate.create('removed', '==', 0);
        }

        function create(initialValues) {
            return this.manager.createEntity(entityName, initialValues);
        }

        function getAll(forceRemote) {
            self = this;
            var orderBy = 'startDate desc';
           
            var sessions;

            if (self._areItemsLoaded() && !forceRemote) {
                sessions = self._getAllLocal(entityName, orderBy, _notRemovedPredicate());
                return self.$q.when(sessions);
            }

            return EntityQuery.from('Sessions')
                .where(_notRemovedPredicate())
                .orderBy(orderBy)
                .expand("lab")
                .toType(entityName)
                .using(self.manager).execute()
                .then(querySucceeded, self._queryFailed);

            function querySucceeded(data) {
                sessions = data.results;
                self._areItemsLoaded(true);
                self.log('Retrieved [Sessions] from remote data source', sessions.length, true);
                return sessions;
            }
        }

        function getById(id, forceRemote) {
            return this._getById(entityName, id, forceRemote);
        }

        function getCount() {
            self = this;
            if (self._areItemsLoaded()) {
                return self.$q.when(self._getLocalEntityCount(entityName,_notRemovedPredicate()));
            }

            return EntityQuery.from('Sessions').where(_notRemovedPredicate()).take(0).inlineCount()
                .using(self.manager).execute()
                .then(self._getInlineCount);
        }

        function getFirstByDate() {
            self = this;
            var orderBy = 'startDate desc';
            return self._getFirstByDate('Sessions', orderBy);
        }

        function getParticipant(session, participant) {
            if (self === undefined) {
                self = this;
            }
            var orderBy = 'identity.email';
            var predicate = Predicate.and([_sessionPredicate(session.sessionId), _participantPredicate(participant.identityId)]);
            var participants = self._getAllLocal(model.entityNames.sessionUser, orderBy, predicate);
            if (participants !== undefined && participants != null && participants.length == 1) {
                return participants[0];
            }
            return undefined;
            
            function _participantPredicate(filterValue) {
                return Predicate.create('identityId', '==', filterValue);
            }
            
            function _sessionPredicate(filterValue) {
                return Predicate.create('sessionId', '==', filterValue);
            }
        }

        function getUpcomingThreeSessions() {
            self = this;
            var orderBy = 'startDate';

            var predicate = Predicate.and([_notRemovedPredicate(),_upcomingThreeSessionsPredicate()]);

            return  self._getAllLocalWithTake(entityName, orderBy, predicate, 3);

            function _upcomingThreeSessionsPredicate() {
                return Predicate.create('startDate', '>=', moment().toDate());
            }
        }

        function isParticipantInSession(session, participant) {
            self = this;
            var sessionParticipant = getParticipant(session, participant);

            return (sessionParticipant !== undefined);
        }

        function removeParticipant(session, participant) {
            var internalParticipant = getParticipant(session, participant);
            if (internalParticipant !== undefined) {
                internalParticipant.entityAspect.setDeleted();
            }
        }
    }
})();