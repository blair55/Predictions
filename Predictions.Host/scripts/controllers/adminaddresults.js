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
		
	$http.get('/api/admin/getfixturesawaitingresults').success(function(data){
		$scope.isLoaded = true;
		$scope.model = data;
	});

	$scope.submitResult = function(row, index){
		var result = {
			fixtureId: row.fxId,
			score: {
				home: row.homeScore,
				away: row.awayScore
			}};

		$http.post('/api/admin/result', result).success(function(data){
    		//$scope.model.rows.splice( index, 1 );
    		row.submitted = true;
		});
	};

});
