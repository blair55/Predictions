'use strict';

/**
 * @ngdoc directive
 * @name frontendApp.directive:submitscore
 * @description
 * # submitscore
 */
angular.module('frontendApp')
	.directive('submitscore', function ($http, notify) {
	    return {
	        templateUrl: 'views/directives/submitscore.html',
	        restrict: 'E',
	        scope: {
	            row: '=',
	            postUrl: '@'
	        },
	        link: function postLink(scope, element, attrs) {
	            //element.text('this is the submitScore directive');
	            //console.log("IS");
	            //console.log(scope);

	            function getMessage(row, score) {
	                return ['Successfully submitted ', row.fixture.home, ' ', score.home, ' v ', score.away, ' ', row.fixture.away].join('');
	            }

	            scope.enterEditMode = function (row) {
	                row.editing = true;
	                row.existingScoreOriginal = angular.copy(row.existingScore);
	            };

	            scope.exitEditMode = function (row) {
	                row.editing = false;
	                row.existingScore = row.existingScoreOriginal;
	            };

	            scope.createScore = function (row) {
	                var msg = getMessage(row, row.newScore);
	                var prediction = {
	                    fixtureId: row.fixture.fxId,
	                    score: row.newScore
	                };
	                scope.inSubmission = true;
	                $http.post(scope.postUrl, prediction).success(function (data) {
	                    notify.success(msg);
	                    row.scoreSubmitted = true;
	                    row.existingScore = row.newScore;
	                    scope.inSubmission = false;
	                }).error(function (data, status, headers, config) {
	                    scope.inSubmission = false;
	                });
	            };

	            scope.editScore = function (row) {
	                var msg = getMessage(row, row.existingScore);
	                var prediction = {
	                    fixtureId: row.fixture.fxId,
	                    score: row.existingScore
	                };
	                scope.inSubmission = true;
	                $http.post(scope.postUrl, prediction).success(function (data) {
	                    notify.success(msg);
	                    row.existingScoreOriginal = row.existingScore;
	                    scope.exitEditMode(row);
	                    scope.inSubmission = false;
	                }).error(function (data, status, headers, config) {
	                    scope.inSubmission = false;
	                });
	            };
	        },
	    };
	});