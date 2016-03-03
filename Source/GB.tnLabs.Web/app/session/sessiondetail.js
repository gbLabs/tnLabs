(function () {
    'use strict';
    var controllerId = 'sessiondetail';
    angular.module('app').controller(controllerId, ['$cookies', '$scope', '$routeParams', '$window', 'common', 'config', 'datacontext', 'spinner', 'WizardHandler', sessions]);

    function sessions($cookies, $scope, $routeParams, $window, common, config, datacontext, spinner, WizardHandler) {
        var vm = this;
        var trainerId;
        vm.filteredLabs = [];
        vm.session = undefined;
        vm.selectedParticipant = undefined;
        vm.filteredParticipants = [];
        vm.filteredTrainers = [];
        vm.selectedParticipants = [];
        vm.selectedTrainer = [];
        vm.trainer = [];
        vm.startTime = '';
        vm.endTime = '';

        vm.addParticipantToSession = addParticipantToSession;
        vm.addTrainerToSession = addTrainerToSession;
        vm.createSession = createSession;
        vm.estimatedPrice = undefined;
        vm.gotoLabs = gotoLabs;
        vm.gotoSchedule = gotoSchedule;
        vm.gotoSummary = gotoSummary;
        vm.gotoParticipants = gotoParticipants;
        vm.removeParticipant = removeParticipant;
        vm.removeTrainer = removeTrainer;


        activate();

        function activate() {
            onDestroy();
            common.activateController([initSession(), getLabs(true), getParticipants(true), getTrainers()], controllerId);

            $scope.$on('wizard:stepChanged',
                function () {
                    vm.errors = [];
                });
        }

        function addParticipantToSession() {

            if (vm.selectedParticipant !== undefined) {
                if (typeof (vm.selectedParticipant) !== "string" && !datacontext.session.isParticipantInSession(vm.session, vm.selectedParticipant)) {
                    datacontext.sessionuser.create(
                        {
                            identity: vm.selectedParticipant,
                            session: vm.session,
                        });
                    vm.selectedParticipant = undefined;
                }
            }
        }

        function addTrainerToSession() {
            if (vm.selectedTrainer != null) {
                vm.trainer = vm.selectedTrainer;
                vm.session.trainerId = vm.trainer.identityId;
                vm.selectedTrainer = null;
            }
        }

        function createSession() {

            spinner.spinnerShow();
            //var subscription = $cookies.subscription;
            //vm.session.subscriptionId = subscription;
            datacontext.saveChanges().then(saveSucceded, saveFailed);

            function saveSucceded() {
                spinner.spinnerHide();
                $window.history.back();
            }

            function saveFailed(error) {
                logError('Unable to create session ' + error);
                spinner.spinnerHide();
            }
        }

        function initSession() {
            var val = $routeParams.id;
            if (val === 'new') {
                vm.isNew = true;
                var sDate = moment().add(1, 'hours').minutes(0).seconds(0);
                var eDate = moment().add(2, 'hours').minutes(0).seconds(0);

                vm.session = datacontext.session.create({ startDate: sDate.toDate(), endDate: eDate.toDate(), vmSize: 'Small' });
                return;
            }
            vm.isNew = false;
            datacontext.session.getById(val)
                .then(function (data) {
                    vm.session = data;
                    vm.session.entityAspect.loadNavigationProperty("sessionUsers");
                    getSessionTrainer(vm.session.trainerId);
                    //if it's an edit, we need to keep the original values to calcuate the available resources later
                    vm.originalNumberOfUsers = vm.session.sessionUsers.length;
                    vm.originalVMSize = vm.session.vmSize;
                }, function (error) {
                    logError('Unable to get session ' + val);
                    gotoSessions();
                });

            function gotoSessions() {
                $location.path('/sessions');
            }
        }

        function getLabs(forceRefresh) {
            return datacontext.lab.getAll(forceRefresh)
                .then(function (data) {
                    return vm.filteredLabs = data;
                });
        }

        function getParticipants(forceRefresh) {
            return datacontext.identity.getAll(forceRefresh)
               .then(function (data) {
                   return vm.filteredParticipants = data;
               });
        }

        function getTrainers() {
            return datacontext.identity.getTrainers()
               .then(function (data) {
                   return vm.filteredTrainers = data;
               });
        }

        function getSessionTrainer(trainerId) {
            return datacontext.identity.getById(trainerId)
               .then(function (data) {
                   return vm.trainer = data;
               });
        }

        function gotoLabs() {
            var aspect = vm.session.entityAspect;

            if (aspect.validateProperty('sessionName')) {
                WizardHandler.wizard().next();
            }
        }

        function gotoParticipants() {

            var aspect = vm.session.entityAspect;
            //validate if lab was selected if yes go to next step
            if (aspect.validateEntity()) {
                WizardHandler.wizard().next();
            }
        }

        function gotoSchedule() {
            var aspect = vm.session.entityAspect;
            //validate if lab was selected if yes go to next step
            if (!aspect.validateProperty('lab')) {
                vm.errors = aspect.getValidationErrors('lab');
                if (vm.errors.length > 0) {
                    vm.errors[0].errorMessage = 'Please select a lab';
                }
            } else {
                vm.errors = [];
                WizardHandler.wizard().next();
            }
        }

        function gotoSummary() {
            spinner.spinnerShow();

            var validNbOfParticipants = checkSessionHasParticipants();
            var validTrainer = checkSessionHasTrainer();
            if (!validNbOfParticipants || !validTrainer)
            {
                spinner.spinnerHide();
                return;
            }

            datacontext.getAvailableSubscriptionResources(vm.session.startDate, vm.session.endDate)
                .then(function (availableCPUs) {
                    if (!vm.isNew) {
                        availableCPUs = availableCPUs + vm.originalNumberOfUsers * common.vmCoreNumber(vm.originalVMSize);
                    }

                    var participantsNo = vm.session.sessionUsers.length;
                    var sessionCPUs = participantsNo * common.vmCoreNumber(vm.session.vmSize);

                    if (sessionCPUs > availableCPUs) {
                        vm.errors.push({ errorMessage: buildErrorMessage(vm.session, sessionCPUs, availableCPUs) });
                    } else {

                        vm.estimatedPrice = calculateEstimatedPrice();
                        WizardHandler.wizard().next();
                    }
                    spinner.spinnerHide();
                }, function (error) {
                    spinner.spinnerHide();
                });

            function buildErrorMessage(session, sessionCPUs, availableCPUs) {

                var errorMessage = 'Session requires ' + sessionCPUs + ' computing cores. Only ' + availableCPUs + ' computing cores are available when the session is schedule.';

                if (common.vmCoreNumber(vm.session.vmSize) === 1) {
                    errorMessage += ' Please reschedule the session.';
                }
                else {
                    errorMessage += ' Please change computing size or reschedule the session.';
                }

                return errorMessage;

            }

            function calculateEstimatedPrice() {
                var vmPrice = common.vmPricingHour(vm.session.vmSize);
                var participantsNo = vm.session.sessionUsers.length;

                var duration = moment.duration(vm.session.duration + 30, 'minutes');
                var hours = duration.hours();

                if (duration.minutes() > 0) {
                    hours = hours + 1;
                }
                return (participantsNo * hours * vmPrice).toFixed(2);
            }

            function checkSessionHasParticipants() {
                vm.errors = [];
                var numberOfParticipants = vm.session.sessionUsers.length;
                if (numberOfParticipants == 0) {
                    vm.errors.push({ errorMessage: 'Each session require at least one participant.' });
                    return false;
                }

                return true;
            }

            function checkSessionHasTrainer() {
                if (vm.session.trainerId == null) {
                    vm.errors.push({ errorMessage: 'Each session require one trainer.' });
                    return false;
                }
                return true;
            }
        }

        function onDestroy() {
            $scope.$on('$destroy', function () {
                datacontext.cancel();
            });
        }

        function removeParticipant(participant) {
            datacontext.session.removeParticipant(vm.session, participant);
        }

        function removeTrainer() {
            vm.session.trainerId = null;
            vm.selectedTrainer = null;
            vm.trainer = null;
        }
    }
})();
