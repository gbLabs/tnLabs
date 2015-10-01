(function () {
	'use strict';

	var serviceId = 'vm.management';

	angular.module('app').factory(serviceId, ['$http', vmManagement]);

	function vmManagement($http) {
		var service = {
			createVmForBaseImage: createVmForBaseImage
		};

		return service;

		function createVmForBaseImage(imageName, imageDescription, newImageName, chocoPackages) {
			return $http.get('api/azuremanagement/CreateBaseVmImage?imageName=' + encodeURIComponent(imageName) +
				'&imageDescription=' + encodeURIComponent(imageDescription) +
				'&newImageName=' + encodeURIComponent(newImageName) +
				'&chocoPackages=' + encodeURIComponent(chocoPackages));
		}
	}
})();