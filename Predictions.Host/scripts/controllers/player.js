'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:PlayerCtrl
 * @description
 * # PlayerCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
  .controller('PlayerCtrl', function ($scope, $http, $routeParams) {
  	var url = '/api/player/' + $routeParams.playerName;
    $http.get(url).success(function(data){
    	$scope.isLoaded = true;
    	$scope.model = data;
    });
  });
