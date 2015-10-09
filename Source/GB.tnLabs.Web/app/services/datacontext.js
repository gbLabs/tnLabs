(function () {
    'use strict';

    var serviceId = 'datacontext';
    angular.module('app').factory(serviceId,
        ['common', 'entityManagerFactory', 'model', 'repositories', 'spinner', datacontext]);

	function datacontext(common, emFactory, model, repositories, spinner) {
        var entityNames = model.entityNames;
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(serviceId);
        var logError = getLogFn(serviceId, 'error');
        var logSuccess = getLogFn(serviceId, 'success');
        var manager = emFactory.newManager();
		var repoNames = ['lab', 'session', 'sessionuser', 'user'];
        var $q = common.$q;
        var primePromise;
        var version = undefined;

        var storeMeta = {
            isLoaded: {
                sessions: false,
                labs: false
            }
        };

        var service = {
            cancel: cancel,
            deleteEntityNoSave: deleteEntityNoSave,
            deleteEntity: deleteEntity,
            saveChanges: saveChanges,
            prime: prime,
			availableVmImages: [],
			availableChocoPackages: [],
			getVersion: getVersion,
			getAvailableSubscriptions: getAvailableSubscriptions,
			getAvailableSubscriptionResources: getAvailableSubscriptionResources,
            sendInvites: sendInvites
        };

        init();

        return service;

        function init() {
			repositories.init(manager);
			defineLazyLoadedRepos();
			getAvailableVmImages();
			getAvailableChocoPackages();
			getAvailableSubscriptions();
		    setupEventForHasChangesChanged();

		}
        
		function cancel() {
		    if (manager.hasChanges()) {
		        manager.rejectChanges();
		        logSuccess('Canceled changes', null, true);
		    }
		}

		function getVersion(forceRemote) {
		    var self = this;
		    if (version === undefined || forceRemote)
		    {
		      return breeze.EntityQuery
		        .from('Version')
		        .using(manager)
		        .execute()
		        .then(getVersionSucceeded);
		    }

		    return self.$q.then(version);
		}

		function getAvailableVmImages() {
		    return breeze.EntityQuery
		        .from('AvailableTemplateImages')
		        .using(manager)
		        .execute()
		        .then(getAvailableVmImagesSucceeded);
		}

		function getAvailableChocoPackages() {
		    return breeze.EntityQuery
		        .from('AvailableChocoPackages')
		        .using(manager)
		        .execute()
		        .then(getAvailableChocoPackagesSucceeded);
		}

		function getAvailableSubscriptions() {
		    return breeze.EntityQuery
		        .from('AvailableSubscriptions')
		        .using(manager)
		        .execute()
		        .then(getAvailableSubscriptionsSucceeded);
		}

		function getAvailableSubscriptionResources(sessionStartDate,sessionEndDate) {
		    return breeze.EntityQuery
                .from('CheckForAvailableResources')
                .withParameters({ sessionStart: moment(sessionStartDate).format(), sessionEnd: moment(sessionEndDate).format() })
                        .using(manager)
                        .execute()
                        .then(getAvailableSubscriptionsResourcesSucceeded);
		}

		function sendInvites($http, value) {
		    $http({
		        url: "api/Manage/SendInvites",
		        method: "POST",
		        data: $.param({ value })
		    }).success(function (data) {
		        onInvitesSent(data);
		    });
		}

		function getVersionSucceeded(data) {
		    version = data.results[0];
		    log('Retrieved version'+service.version+'from remote data source',null, true);
		    return version;
		}

		function getAvailableVmImagesSucceeded(data) {
		    service.availableVmImages = data.results;
		    log('Retrieved [VM Images] from remote data source', service.availableVmImages.length, true);
			// datacontext was defined earlier in the module.
		}

		function getAvailableChocoPackagesSucceeded(data) {
		    service.availableChocoPackages = data.results;
		    log('Retrieved [Application packages] from remote data source', service.availableChocoPackages.length, true);

			// datacontext was defined earlier in the module.
		}

		function getAvailableSubscriptionsSucceeded(data) {
			service.availableSubscriptions = data.results;
			log('Retrieved [Available subscriptions] from remote data source', service.availableSubscriptions.length, true);
		    return service.availableSubscriptions;
		}

		function getAvailableSubscriptionsResourcesSucceeded(data) {
		    service.availableSubscriptionResources = data.results;
		    log('Retrieved [Available subscriptions] from remote data source', service.availableSubscriptionResources.length, true);
		    return service.availableSubscriptionResources;
		}

        function defineLazyLoadedRepos() {
			repoNames.forEach(function (name) {
                Object.defineProperty(service, name, {
                    configurable: true, //will redefine this property once
					get: function () {
                        // The 1st time the repo is request via this property, 
                        // we ask the repositories for it (which will inject it).
                        var repo = repositories.getRepo(name);
                        // Rewrite this property to always return this repo;
                        // no longer redefinable
                        Object.defineProperty(service, name, {
                            value: repo,
                            configurable: false,
                            enumerable: true
                        });
                        return repo;
                    }
                });
            });
        }

        function prime() {
            if (primePromise) return primePromise;

            primePromise = $q.all([getVersion(true),service.lab.getAll(true), service.session.getAll(true),
                    getAvailableVmImages(),
                    getAvailableChocoPackages(),
					getAvailableSubscriptions(),
                    service.sessionuser.getAll(true)])
                .then(extendMetadata)
                .then(success);
            return primePromise;

            function success() {
                log('Primed the data');
            }

            function extendMetadata() {
                var metadataStore = manager.metadataStore;
                model.extendMetadata(metadataStore);
                var types = metadataStore.getEntityTypes();
				types.forEach(function (type) {
                    if (type instanceof breeze.EntityType) {
                        set(type.shortName, type);
                    }
                });

                function set(resourceName, entityName) {
                    metadataStore.setEntityTypeForResourceName(resourceName, entityName);
                }
            }

        }

        function saveChanges() {
            //Must return a promis otherwise lab and session will fail
            return manager.saveChanges()
                .then(saveSucceded);

        }

        function saveSucceded() {
            log('All changes where saved', "", true);
        }

        function setupEventForHasChangesChanged() {
            manager.hasChangesChanged.subscribe(function (eventArgs) {
                var data = { hasChanges: eventArgs.hasChanges };
                // send the message (the ctrl receives it)
                common.$broadcast(events.hasChangesChanged, data);
            });
        }

        function deleteEntity(entity) {
            entity.entityAspect.setDeleted();
            saveChanges();
        }
        
        function deleteEntityNoSave(entity) {
            entity.entityAspect.setDeleted();
        }
       
    }
})();