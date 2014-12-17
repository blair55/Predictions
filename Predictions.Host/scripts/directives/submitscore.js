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

	            var state = {
	                readonly: 'readonly', create: 'create', edit: 'edit'
	            };

	            function getMessage(row, score) {
	                return ['Successfully submitted ', row.fixture.home, ' ',
                        score.home, ' - ', score.away, ' ', row.fixture.away].join('');
	            }

	            function decideInitialState() {
	                scope.row.state = scope.row.scoreSubmitted ? state.readonly : state.create;
	                //scope.row.isSubmittable = false;
	            }

	            decideInitialState();

	            var createScoreForm = null;
	            var editScoreForm = null;

	            scope.setCreateScoreForm = function (form) {
	                createScoreForm = form;
	            }

	            scope.setEditScoreForm = function (form) {
	                editScoreForm = form;
	            }

	            scope.row.submit = function (row) {
	                scope.row.submitWithCallBack(row, function () { });
	            }

	            scope.row.submitWithCallBack = function (row, cb) {
	                switch (row.state) {
	                    case state.create:
	                        if (createScoreForm.$valid) {
	                            createScore(row, cb);
	                        }
	                        else {
	                            cb();
	                        }
	                        break;
	                    case state.edit:
	                        if (editScoreForm.$valid) {
	                            editScore(row, cb);
	                        }
	                        else {
	                            cb();
	                        }
	                        break;
	                    case state.readonly:
	                        cb();
	                        break;
	                }
	            };

	            scope.enterEditMode = function (row) {
	                row.state = state.edit;
	                row.existingScoreOriginal = angular.copy(row.existingScore);
	            };

	            scope.enterReadOnlyMode = function (row) {
	                row.state = state.readonly;
	                row.existingScore = row.existingScoreOriginal;
	            };

	            var createScore = function (row, cb) {
	                var msg = getMessage(row, row.newScore);
	                var prediction = {
	                    fixtureId: row.fixture.fxId,
	                    score: row.newScore
	                };
	                scope.inSubmission = true;
	                $http.post(scope.postUrl, prediction).success(function (data) {
	                    notify.success(msg);
	                    scope.enterReadOnlyMode(row);
	                    row.existingScore = row.newScore;
	                    scope.inSubmission = false;
	                    cb();
	                }).error(function (data, status, headers, config) {
	                    scope.inSubmission = false;
	                    cb();
	                });
	            };

	            var editScore = function (row, cb) {
	                var msg = getMessage(row, row.existingScore);
	                var prediction = {
	                    fixtureId: row.fixture.fxId,
	                    score: row.existingScore
	                };
	                scope.inSubmission = true;
	                $http.post(scope.postUrl, prediction).success(function (data) {
	                    notify.success(msg);
	                    row.existingScoreOriginal = row.existingScore;
	                    scope.enterReadOnlyMode(row);
	                    scope.inSubmission = false;
	                    cb();
	                }).error(function (data, status, headers, config) {
	                    scope.inSubmission = false;
	                    cb();
	                });
	            };
	        }
	    };
	});