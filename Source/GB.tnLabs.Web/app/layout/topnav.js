(function () {
	'use strict';

	var controllerId = 'topnav';
	angular.module('app').controller(controllerId,
		['$route', 'config','common', 'datacontext', topnav]);

	function topnav($route, config,common, datacontext) {
		var vm = this;
		var getLogFn = common.logger.getLogFn;
		var log = getLogFn(controllerId);

		vm.changeSubscription = changeSubscription;
		vm.currentStyle = currentStyle;
		vm.subscriptions = [];



		activate();

		function activate() {
		    common.activateController([getSubscriptions()], controllerId)
               .then(function () {

                   log('Activated Subscriptions View');
               });
			
		}

		function getSubscriptions() {
		    
		    return datacontext.getAvailableSubscriptions()
               .then(function (data) {
                   return vm.subscriptions = data;
               });
		    
		
		}

		function changeSubscription(subscriptionId) {
			//TODO: implement
			//alert(subscriptionId);
		}

		function currentStyle(isCurrent) {
			if (!isCurrent) {
				return '';
			}
			else {
				return 'fa-check active';
			}
		}
	};
})();