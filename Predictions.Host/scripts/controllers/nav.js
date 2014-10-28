'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:NavCtrl
 * @description
 * # NavCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
  .controller('NavCtrl', function ($scope, $location, auth) {
      $scope.navCollapsed = true;
      auth.withPlayer(function (player) {
          $scope.player = player;
      });
      $scope.navTo = function ($event) {
          $scope.navCollapsed = true;
          var i = $event.target.href.indexOf('#') + 1;
          var path = $event.target.href.substring(i);
          $location.path(path);
          $event.preventDefault();
      }
  });