'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:FixtureCtrl
 * @description
 * # FixtureCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
  .controller('FixtureCtrl', function ($scope, $http, $routeParams) {
  	var url = '/api/fixture/' + $routeParams.fxid;
	$http.get(url).success(function(data){
		$scope.isLoaded = true;
		$scope.model = data;
	});
  });
