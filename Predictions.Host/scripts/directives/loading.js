'use strict';

/**
 * @ngdoc directive
 * @name frontendApp.directive:loading
 * @description
 * # loading
 */
angular.module('frontendApp')
  .directive('loading', function () {
      return {
          templateUrl: 'views/directives/loading.html',
          restrict: 'E',
          scope: {
              noRowsMsg: "@",
              model: "="
          }
      };
  });