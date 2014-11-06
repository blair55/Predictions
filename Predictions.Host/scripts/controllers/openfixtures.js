'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:OpenfixturesCtrl
 * @description
 * # OpenfixturesCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
	.controller('OpenfixturesCtrl', function ($scope, $http) {
	    $http.get('/api/openfixtures').success(function (data) {
	        $scope.model = data;
	    });
	});