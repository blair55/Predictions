'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:GameweekscoresCtrl
 * @description
 * # GameweekscoresCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
  .controller('GameweekscoresCtrl', function ($scope, $http, $routeParams) {
  	var url = '/api/gameweekscores/' + $routeParams.gameWeekNo;
	$http.get(url).success(function(data){
		$scope.isLoaded = true;
		$scope.model = data;
	});
});