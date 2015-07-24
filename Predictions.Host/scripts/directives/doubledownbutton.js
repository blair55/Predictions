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
            template:
                '<span ng-if="row.isDoubleDown" class="label label-warning">' +
                    '<i class="glyphicon glyphicon-ok"></i>&nbsp;Double Down</span>' +
                    '<span ng-if="!row.isDoubleDown" class="label double-down-button" ng-click="row.setAsDoubleDown(row)">' +
                    '<i class="glyphicon glyphicon-ok"></i>&nbsp;Double Down</span>',
            restrict: 'E',
            scope: {
                row: '='
            }
        };
    });