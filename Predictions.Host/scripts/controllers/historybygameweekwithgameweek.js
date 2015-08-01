'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:HistorybygameweekwithgameweekCtrl
 * @description
 * # HistorybygameweekwithgameweekCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
  .controller('HistorybygameweekwithgameweekCtrl', function ($scope, $http, $routeParams, auth, title) {
      var url = '/api/leaguehistory/' + $routeParams.leagueId + '/gameweek/' + $routeParams.gameweekno;
      $http.get(url).success(function (data) {
          $scope.model = data;
          title.set(data.league.name + " / GW#" + data.gameWeekNo + " Table");
      });
      var neighboursUrl = '/api/gameweekneighbours/' + $routeParams.gameweekno;
      $http.get(neighboursUrl).success(function (data) {
          $scope.neighbours = data;
      });
      auth.withPlayer(function (player) {
          $scope.loggedInPlayer = player;
      });
  });
