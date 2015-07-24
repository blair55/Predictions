'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:MainCtrl
 * @description
 * # MainCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
  .controller('MainCtrl', function ($scope, $http, auth) {
      auth.withPlayer(function (player) {
          $scope.loggedInPlayer = player;
      });
      $http.get('/api/getleaguepositionforplayer').success(function (data) {
          $scope.global = data;
      });
      $http.get('/api/getlastgameweekandwinner').success(function (data) {
          $scope.lastgw = data;
      });
      $http.get('/api/getopenfixtureswithnopredictionsforplayercount').success(function (data) {
          $scope.openfixturecount = data;
      });
      $http.get('/api/leagues').success(function (data) {
          $scope.leagues = data;
      });
  });