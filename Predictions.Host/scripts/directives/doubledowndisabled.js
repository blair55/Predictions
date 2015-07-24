'use strict';

/**
 * @ngdoc directive
 * @name frontendApp.directive:doubledowndisabled
 * @description
 * # doubledowndisabled
 */
angular.module('frontendApp')
    .directive('doubledowndisabled', function() {
        return {
            template:
                '<span class="label double-down-button dd-disabled">' +
                    '<i class="glyphicon glyphicon-ok"></i>&nbsp;Double Down</span>',
            restrict: 'E'
        };
    });