'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:NavCtrl
 * @description
 * # NavCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
  .controller('NavCtrl', function ($scope, auth) {
  	auth.withPlayer(function(player){
    	$scope.player = player;  		
  	});
  });
