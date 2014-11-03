'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:HistorybygameweekCtrl
 * @description
 * # HistorybygameweekCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
  .controller('HistorybygameweekCtrl', function ($scope, $http) {
      var url = '/api/history/gameweek';
      $http.get(url).success(function (data) {
          $scope.model = data;
      });
  });
