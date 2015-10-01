(function () {
    'use strict';
    var controllerId = 'labs';
    angular.module('app').controller(controllerId, ['$location','$routeParams', 'common', 'config', 'datacontext', labs]);

    function labs($location,$routeParams, common, config, datacontext) {
        // Using 'Controller As' syntax, so we assign this to the vm variable (for viewmodel).
        var vm = this;
        var getLogFn = common.logger.getLogFn;
        var log = getLogFn(controllerId);

        vm.filteredLabs = [];
        
        vm.title = 'Labs';
        vm.gotoLab = gotoLab;
        vm.removeLab = removeLab;

        activate();

        function activate() {
            common.activateController([getLabs()], controllerId)
                .then(function () {
                    
                     log('Activated Labs View');
                });
        }
        
        function getLabs(forceRefresh) {
            return datacontext.lab.getAll(forceRefresh)
                .then(function(data) {
                    return vm.filteredLabs = data;
                });
        }
        

        function gotoLab(lab) {
            if (lab && lab.labId) {
                $location.path('/lab/' + lab.labId);
            }
        }
        
        function removeLab(lab) {
            lab.removed = true;
            datacontext.saveChanges();
            getLabs(false);
        }
        
      
    }
})();