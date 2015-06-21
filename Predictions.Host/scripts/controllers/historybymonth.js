'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:HistorybymonthCtrl
 * @description
 * # HistorybymonthCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
  .controller('HistorybymonthCtrl', function ($scope, $http, $routeParams) {
      var url = '/api/leaguehistory/' + $routeParams.leagueId + '/month';
      $http.get(url).success(function (data) {
          $scope.model = data;
      });
  });
