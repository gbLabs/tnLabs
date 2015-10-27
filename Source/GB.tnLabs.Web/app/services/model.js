(function () {
    'use strict';

    // Controller name is handy for logging
    var serviceId = 'model';

    // Define the controller on the module.
    // Inject the dependencies. 
    // Point to the controller definition function.
    angular.module('app').factory(serviceId, ['model.validation', model]);

    function model(modelValidation) {
        // Define the functions and properties to reveal.
        var entityNames = {
            lab: 'Lab',
            session: 'Session',
            sessionUser: 'SessionUser',
            identity: 'Identity'
        };

        var service = {
            configureMetadataStore: configureMetadataStore,
            entityNames: entityNames,
            extendMetadata: extendMetadata
        };

        return service;
        
        function configureMetadataStore(metadataStore) {
            registerLab(metadataStore);
            registerSession(metadataStore);
            registerSessionUser(metadataStore);
            registerIdentity(metadataStore);
            modelValidation.createAndRegister(entityNames);
        }
        
        function extendMetadata(metadataStore) {
            modelValidation.applyValidators(metadataStore);
        }

        function registerLab(metadataStore) {
            metadataStore.registerEntityTypeCtor('Lab', Lab);
            
            function Lab() {
                
            }
        }
        
        function registerSession(metadataStore) {
            metadataStore.registerEntityTypeCtor('Session', Session);

            function Session() {
          
            }

            Object.defineProperty(Session.prototype, 'date', {
                get: function () {
                    return this.startDate;
                },
                set: function (value) {
                    var date = moment(value);
                    var mStartDate = moment(this.startDate)
                        .day(date.day())
                        .month(date.month())
                        .year(date.year());
                    var mEndDate = moment(this.endDate)
                       .day(date.day())
                       .month(date.month())
                       .year(date.year());
                    //for now we only support session that start and end on the same day.
                    this.startDate = mStartDate.toDate();
                    this.endDate = mEndDate.toDate();

                }
            });

            Object.defineProperty(Session.prototype, 'startTime', {
                get: function () {
                    return moment(this.startDate).format('HH:mm');
                },
                set: function (value) {
                    var sTime = moment(value, 'hh:mm');
                    var mStartDate = moment(this.startDate).hour(sTime.hour()).minute(sTime.minute());
                    this.startDate = mStartDate.toDate();
                }
            });
            
            Object.defineProperty(Session.prototype, 'endTime', {
                get: function () {
                    return moment(this.endDate).format('HH:mm');
                },
                set: function(value) {
                    var eTime = moment(value, 'hh:mm');
                    var mEndDate = moment(this.startDate).hour(eTime.hour()).minute(eTime.minute());
                    var sDate = moment(this.startDate);
                    if (sDate.isAfter(mEndDate)) return;
                    this.endDate = mEndDate.toDate();
                }
            });

            Object.defineProperty(Session.prototype, 'duration', {
                get: function () {
                    return moment(this.endDate).diff(moment(this.startDate), 'minutes');
                }
            });

            Object.defineProperty(Session.prototype, 'isPassed', {
                get: function () {
                    return moment().isAfter(this.startDate);
                }
            });
            
            Object.defineProperty(Session.prototype, 'formattedStartDateLabel', {
                get: function () {
                    return this.isPassed ? 'Session has started in' : 'Session will start in';
                }
            });
            
            Object.defineProperty(Session.prototype, 'formattedDurationLabel', {
                get: function () {
                    return this.isPassed ? 'Session has lasted' : 'Session will last';
                }
            });

            Object.defineProperty(Session.prototype, 'formattedStartDate', {
                get: function () {
                    return moment(this.startDate).format('Do MMMM YYYY') + ' at ' + moment(this.startDate).format('HH:mm');
                }
            });
            
            Object.defineProperty(Session.prototype, 'formattedDuration', {
                get: function () {
                    var duration = moment.duration(this.duration, 'minutes');
                    var hours = duration.hours();
                    var minutes = duration.minutes();
                    var formattedDuration = '';
                    if (hours > 0) {
                        formattedDuration = moment.duration(hours, 'hours').humanize();
                    }
                    if (minutes > 0) {
                        if (formattedDuration.length > 0) {
                            formattedDuration = formattedDuration + ' and ';
                        }
                        formattedDuration = formattedDuration  + moment.duration(minutes, 'minutes').humanize();
                    }
                    return formattedDuration;
                }
            });
        }
        
        function registerSessionUser(metadataStore) {
            metadataStore.registerEntityTypeCtor('SessionUser', SessionUser);

            function SessionUser() {
                
            }
        }
        
        function registerIdentity(metadataStore) {
            metadataStore.registerEntityTypeCtor('Identity', Identity);

            function Identity() {
            }

            Object.defineProperty(Identity.prototype, 'fullName', {
                get: function () {
                    var fn = this.firstName;
                    var ln = this.lastName;
                    return ln ? fn + ' ' + ln : fn;
                }
            });
        }
    }
})();
