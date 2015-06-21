'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:HistorybygameweekwithgameweekCtrl
 * @description
 * # HistorybygameweekwithgameweekCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
  .controller('HistorybygameweekwithgameweekCtrl', function ($scope, $http, $routeParams) {
      var url = '/api/leaguehistory/' + $routeParams.leagueId + '/gameweek/' + $routeParams.gameweekno;
      $http.get(url).success(function (data) {
          $scope.model = data;
      });
  });
