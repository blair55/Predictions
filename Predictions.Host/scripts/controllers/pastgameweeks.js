'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:PastgameweeksCtrl
 * @description
 * # PastgameweeksCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
  .controller('PastgameweeksCtrl', function ($scope, $http) {
      $http.get('/api/history/gameweek').success(function (data) {
          $scope.model = data;
      });
  });