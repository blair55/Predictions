'use strict';

/**
 * @ngdoc directive
 * @name frontendApp.directive:pagetitle
 * @description
 * # pagetitle
 */
angular.module('frontendApp')
  .directive('pagetitle', function () {
    return {
        template:
            '<h4 class="page-title" ng-if="title" ng-bind="title"></h4>',
        restrict: 'E',
    };
});
