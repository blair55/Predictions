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
		templateUrl: 'views/loading.html',
		restrict: 'E'
    };
  });
