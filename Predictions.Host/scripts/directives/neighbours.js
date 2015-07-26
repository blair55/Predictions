'use strict';

/**
 * @ngdoc directive
 * @name frontendApp.directive:neighbours
 * @description
 * # neighbours
 */
angular.module('frontendApp')
    .directive('neighbours', function() {
        return {
            template:
                '<div class="clearfix" ng-if="neighbours">' +
                    '<div ng-if="neighbours.prev" class="pull-left">' +
                        '<a ng-href="{{url + neighbours.prev}}">&larr; Prev</a>' +
                    '</div>' +
                    '<div ng-if="neighbours.next" class="pull-right">' +
                        '<a ng-href="{{url + neighbours.next}}">Next &rarr;</a>' +
                    '</div>' +
                '</div>',
            restrict: 'A',
            scope: {
                url: '@',
                neighbours: '=?'
            }
        };
    });