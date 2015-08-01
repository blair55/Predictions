'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:PlayerCtrl
 * @description
 * # PlayerCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
  .controller('PlayerCtrl', function ($scope, $http, $routeParams, title) {

      var url = '/api/player/' + $routeParams.playerId + "/" + $routeParams.leagueId;
      $http.get(url).success(function (data) {
          $scope.model = data;
          title.set(data.league.name + " / " + data.player.name);
      });

      var graphUrl = '/api/leaguepositiongraphforplayer/' + $routeParams.playerId + "/" + $routeParams.leagueId;
      $http.get(graphUrl).success(function (data) {
          $scope.labels = data.labels;
          $scope.data = data.data;
          $scope.options = {
              animationSteps: 10,
              //showScale: !window.mobilecheck(),
              showScale: false,
              scaleOverride: true,
              // ** Required if scaleOverride is true **
              // Number - The number of steps in a hard coded scale
              scaleSteps: 16,
              // Number - The value jump in the hard coded scale
              scaleStepWidth: -1,
              // Number - The scale starting value
              scaleStartValue: 17,
          };
      });
  });