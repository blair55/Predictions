'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:PlayergameweekCtrl
 * @description
 * # PlayergameweekCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
  .controller('PlayergameweekCtrl', function ($scope, $http, $routeParams) {
		var url = '/api/playergameweek/' + $routeParams.playerName + '/' + $routeParams.gameWeekNo;
	    $http.get(url).success(function(data){
    	$scope.isLoaded = true;
	    	$scope.model = data;
	    });
  });
