'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:AdminaddresultsforgameweekCtrl
 * @description
 * # AdminaddresultsforgameweekCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
  .controller('AdminaddresultsforgameweekCtrl', function ($scope, $http, $routeParams, title) {
      var url = '/api/admin/getclosedfixturesforgameweek/' + $routeParams.gameweekno;
      $http.get(url).success(function (data) {
          $scope.model = data;
          title.set('GW#' + data.gameWeekNo + " / Add Results");
      });
  });