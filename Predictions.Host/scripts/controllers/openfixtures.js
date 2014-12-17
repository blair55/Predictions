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

	    function submitRow(i) {
	        if (i < $scope.model.rows.length) {
	            var row = $scope.model.rows[i];
	            row.submitWithCallBack(row, function () {
	                submitRow(i + 1);
	            });
	        }
	    }

	    $scope.anyEditableRows = false;

	    $scope.submitAll = function () {
	        submitRow(0);
	    }

	});