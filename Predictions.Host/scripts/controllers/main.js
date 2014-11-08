'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:MainCtrl
 * @description
 * # MainCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
  .controller('MainCtrl', function ($scope, $http) {
      $http.get('/api/getleaguepositionforplayer').success(function (data) {
          $scope.leaguePosition = data;
      });
      $http.get('/api/getlastgameweekandwinner').success(function (data) {
          $scope.lastgw = data;
      });
  });
