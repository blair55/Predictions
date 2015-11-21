'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:HistorybymonthwithmonthCtrl
 * @description
 * # HistorybymonthwithmonthCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
  .controller('HistorybymonthwithmonthCtrl', function ($scope, $http, $routeParams, auth, title) {
      var url = '/api/leaguehistory/' + $routeParams.leagueId + '/month/' + $routeParams.month + '/page/' + $routeParams.page;
      $http.get(url).success(function (data) {
          $scope.model = data;
          title.set(data.league.name + " / " + data.month);
          title.useBackButton('#/history/' + data.league.id + "/month");
      });
      var neighboursUrl = '/api/monthneighbours/' + $routeParams.month;
      $http.get(neighboursUrl).success(function (data) {
          $scope.neighbours = data;
      });
      auth.withPlayer(function (player) {
          $scope.loggedInPlayer = player;
      });
  });
