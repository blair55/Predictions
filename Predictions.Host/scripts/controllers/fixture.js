'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:FixtureCtrl
 * @description
 * # FixtureCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
  .controller('FixtureCtrl', function ($scope, $http, $routeParams) {

      var url = '/api/fixture/' + $routeParams.fxid;
      $http.get(url).success(function (data) {
          $scope.model = data;
      });

      var graphUrl = '/api/fixturepredictiongraph/' + $routeParams.fxid;
      $http.get(graphUrl).success(function (graphData) {
          $scope.labels = graphData.labels;
          $scope.data = graphData.data;
          $scope.options = {
              animationSteps: 10,
              animateRotate: false,
              animateScale: true
          };
      });

      var formGuideUrl = '/api/fixtureformguide/' + $routeParams.fxid;
      $http.get(formGuideUrl).success(function (data) {
          $scope.formGuide = data;
      });

      var previousMeetingsUrl = '/api/fixturepreviousmeetings/' + $routeParams.fxid;
      $http.get(previousMeetingsUrl).success(function (data) {
          $scope.previousMeetings = data;
      });
  });