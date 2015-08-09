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
                '<nav ng-if="neighbours">' +
                    '<ul class="pager">' +
                    '<li class="previous" ng-if="neighbours.prev">' +
                    '<a ng-href="{{url + neighbours.prev}}"><span aria-hidden="true">&larr;</span> Prev</a>' +
                    '</li>' +
                    '<li class="next" ng-if="neighbours.next">' +
                    '<a ng-href="{{url + neighbours.next}}">Next <span aria-hidden="true">&rarr;</span></a>' +
                    '</li>' +
                    '</ul>' +
                    '</nav>',
            restrict: 'A',
            scope: {
                url: '@',
                neighbours: '=?'
            }
        };
    });