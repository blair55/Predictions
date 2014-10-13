'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:LeaguetableCtrl
 * @description
 * # LeaguetableCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
  .controller('LeaguetableCtrl', function ($scope, $http) {
    $http.get('/api/leaguetable').success(function(data){
    	$scope.isLoaded = true;
    	$scope.model = data;
    });
  });
