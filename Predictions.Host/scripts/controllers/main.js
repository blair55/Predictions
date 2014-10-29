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
      $http.get('/api/leagueposition').success(function (data) {
          c3.generate({
              bindto: '#chart',
              data: {
                  columns: data
              }
          });
      });
  });