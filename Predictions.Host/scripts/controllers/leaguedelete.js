'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:LeaguedeleteCtrl
 * @description
 * # LeaguedeleteCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
  .controller('LeaguedeleteCtrl', function ($scope, $http, $routeParams, $location, notify) {

      var url = '/api/league/' + $routeParams.leagueId;

      $http.get(url).success(function (data) {
          $scope.model = data;
      });

      $scope.submit = function () {
          var postUrl = '/api/leaguedelete/' + $scope.model.id;
          $http.post(postUrl).success(function () {
              notify.success("League deleted successfully");
              $location.path('leagues');
          });
      };
  });