'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:HistorybymonthwithmonthCtrl
 * @description
 * # HistorybymonthwithmonthCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
  .controller('HistorybymonthwithmonthCtrl', function ($scope, $http, $routeParams) {
      var url = '/api/history/month/' + $routeParams.month;
      $http.get(url).success(function (data) {
          $scope.model = data;
      });
  });
