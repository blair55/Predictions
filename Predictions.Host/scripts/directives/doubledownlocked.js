'use strict';

/**
 * @ngdoc directive
 * @name frontendApp.directive:doubledownlocked
 * @description
 * # doubledownlocked
 */
angular.module('frontendApp')
    .directive('doubledownlocked', function() {
        return {
            template:
                '<div ng-if="row.isDoubleDown" class="dd dd-selected">' +
                    '<span class="label dd-button">' +
                    '<i class="glyphicon glyphicon-lock"></i>&nbsp;Double Down</span>' +
                    '</div>',
            restrict: 'E',
            scope: {
                row: '='
            }
        };
    });