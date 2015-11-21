'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:InplayCtrl
 * @description
 * # InplayCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
  .controller('InplayCtrl', function ($scope, $http, title) {
      $http.get('/api/inplay').success(function (data) {
          $scope.model = data;
          title.set('Active Game Week');
          title.useBackButton('#/');
      });
  });
