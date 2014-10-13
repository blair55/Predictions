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

    $http.get('/api/openfixtures').success(function(data){
    	$scope.isLoaded = true;
    	$scope.model = data;
    });
    
	$scope.submitResult = function(row, index){
		var prediction = {
			fixtureId: row.fxId,
			score: {
				home: row.homeScore,
				away: row.awayScore
			}};

		$http.post('/api/prediction', prediction).success(function(data){
    		//$scope.model.rows.splice( index, 1 );
    		row.submitted = true;
		});
	};
});