'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:AdminaddresultsCtrl
 * @description
 * # AdminaddresultsCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
	.controller('AdminaddresultsCtrl', function ($scope, $http) {
	    $http.get('/api/admin/getgameweekswithclosedfixtures').success(function (data) {
	        $scope.model = data;
	    });
	});
