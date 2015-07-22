'use strict';

/**
 * @ngdoc directive
 * @name frontendApp.directive:doubledownbutton
 * @description
 * # doubledownbutton
 */
angular.module('frontendApp')
    .directive('doubledownbutton', function() {
        return {
            template: '<span ng-if="row.isDoubleDown" class="label label-warning">Double Down</span><span ng-if="!row.isDoubleDown" class="label label-default" ng-click="row.setAsDoubleDown(row)">Double Down</span>',
            restrict: 'E',
            scope: {
                row: '='
            },
            link: function postLink(scope, element, attrs) {
            }
        };
    });