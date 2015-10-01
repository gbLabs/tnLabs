(function () {
    'use strict';
    var controllerId = 'labdetail';
    angular.module('app').controller(controllerId, ['$cookies', '$location','$routeParams', '$scope', '$window',
        'common', 'config', 'datacontext', 'spinner', 'vm.management', 'WizardHandler', createLab]);

    function createLab($cookies, $location,$routeParams, $scope, $window,  common, config, datacontext, spinner, vmManagement, WizardHandler) {
        var vm = this;
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);


        vm.availableCustomImages = datacontext.availableVmImages;
        vm.availableChocoPackages = datacontext.availableChocoPackages;
        vm.selectedChocoPackages = {};
        vm.createLab = internalCreateLab;
        vm.createTemplateImage = false;
        vm.isNew = true;
        vm.nextStep = nextStep;

        vm.errors = [];
        vm.lab = undefined;

        activate();

        function activate() {
            onDestroy();
            common.activateController([initLab()], controllerId);
        }

        function createTemplateImage() {
        	var selectedChocoPackagesString = '';
        	for (var option in vm.selectedChocoPackages) {
        		if (vm.selectedChocoPackages[option]) {
        			if (selectedChocoPackagesString == '') {
        				selectedChocoPackagesString += option;
        			}
        			else {
        				selectedChocoPackagesString += "," + option;
        			};
        		};
        	};
			
        	return vmManagement.createVmForBaseImage(vm.lab.imageName, vm.lab.description, vm.lab.name,
					selectedChocoPackagesString)
                .success(createSuccess).error(createError);

            function createSuccess() {
                spinner.spinnerHide();
                $window.history.back();
            }

            function createError() {
                spinner.spinnerHide();
            }
        }        

        function initLab() {
            var val = $routeParams.id;
            if (val === 'new') {
                vm.lab = datacontext.lab.create();
                return;
            }
            vm.isNew = false;
            datacontext.lab.getById(val)
                .then(function (data) {
                    vm.lab = data;
                }, function(error) {
                    logError('Unable to get lab ' + val);
                    gotoLabs();
                });
            
            function gotoLabs() {
                $location.path('/labs');
            }
        }

        function internalCreateLab() {
            if (!isValid(vm.lab)) return;
            spinner.spinnerShow();
            if (vm.createTemplateImage) {
                //revert lab changes since we won't actually create the lab
                datacontext.cancel();
                createTemplateImage();
            } else {
            	//var selectedChocoPackagesString = "";
            	//for (var option in vm.selectedChocoPackages) {
            	//	if (vm.selectedChocoPackages[option]) {
            	//		if (selectedChocoPackagesString == '') {
            	//			selectedChocoPackagesString += option;
            	//		}
            	//		else {
            	//			selectedChocoPackagesString += "," + option;
            	//		};
            	//	};
            	//};

                var subscription = $cookies.subscription;
                vm.lab.subscriptionId = subscription;
                vm.lab.creationDate = moment().toDate();
                datacontext.saveChanges().then(saveSucceded, saveFailed);
                
            }
            
            function saveSucceded() {
                spinner.spinnerHide();
                $window.history.back();
            }
            
            function saveFailed() {
                spinner.spinnerHide();
            }
        }
        
        function isValid(lab) {
            var aspect = lab.entityAspect;

            if (!aspect.validateEntity()) {
                vm.errors = aspect.getValidationErrors('imageName');
                if (vm.errors.length > 0) {
                    vm.errors[0].errorMessage = 'Please select an template image';
                    return false;
                }
            }
            return true;
        }

        function nextStep() {
            var aspect = vm.lab.entityAspect;

            if (aspect.validateProperty('name') || aspect.validateProperty('description')) {
                WizardHandler.wizard().next();
            }
        }
        
        function onDestroy() {
            $scope.$on('$destroy', function () {
                datacontext.cancel();
            });
        }
        
        
      
    }
})();