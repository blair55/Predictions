'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:PastgameweeksCtrl
 * @description
 * # PastgameweeksCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
  .controller('PastgameweeksCtrl', function ($scope, $http) {
	$http.get('/api/pastgameweeks').success(function(data){
		$scope.isLoaded = true;
		$scope.model = data;
	});
});
