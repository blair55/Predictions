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

	    $http.get('/api/admin/getfixturesawaitingresults').success(function (data) {
	        $scope.model = data;
	    });

	    $scope.submitResult = function (row, index) {
	        var result = {
	            fixtureId: row.fxId,
	            score: row.score
	        };
	        $http.post('/api/admin/result', result).success(function (data) {
	            row.submitted = true;
	        });
	    };
	});
