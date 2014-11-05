'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:OpenfixturesCtrl
 * @description
 * # OpenfixturesCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
	.controller('OpenfixturesCtrl', function ($scope, $http, notify) {

	    $http.get('/api/openfixtures').success(function (data) {
	        $scope.model = data;
	    });

	    $scope.createScore = function (row) {
	        var msg = getMessage(row, row.newScore);
	        var prediction = {
	            fixtureId: row.fixture.fxId,
	            score: row.newScore
	        };
	        $http.post('/api/prediction', prediction).success(function (data) {
	            notify.success(msg);
	            row.scoreSubmitted = true;
	            row.existingScore = row.newScore;
	        });
	    };

	    $scope.enterEditMode = function (row) {
	        row.editing = true;
	        row.existingScoreOriginal = angular.copy(row.existingScore);
	    }
	    $scope.exitEditMode = function (row) {
	        row.editing = false;
	        row.existingScore = row.existingScoreOriginal;
	    }

	    $scope.editScore = function (row) {
	        var msg = getMessage(row, row.existingScore);
	        var prediction = {
	            fixtureId: row.fixture.fxId,
	            score: row.existingScore
	        };
	        $http.post('/api/prediction', prediction).success(function (data) {
	            notify.success(msg);
	            row.existingScoreOriginal = row.existingScore;
	            $scope.exitEditMode(row);
	        });
	    }

	    function getMessage(row, score) {
	        return ["Successfully submitted ", row.fixture.home, ' ', score.home, ' v ', score.away, ' ', row.fixture.away].join('');
	    }
	});