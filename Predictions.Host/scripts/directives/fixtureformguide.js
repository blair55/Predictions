'use strict';

/**
 * @ngdoc directive
 * @name frontendApp.directive:fixtureformguide
 * @description
 * # fixtureformguide
 */
angular.module('frontendApp')
    .directive('fixtureformguide', function() {
        return {
            template: '<div ng-switch="guide" class="fixture-form-guide">' +
                    '<span ng-switch-when="w" class="label label-success">W</span>' +
                    '<span ng-switch-when="l" class="label label-danger">L</span>' +
                    '<span ng-switch-when="d" class="label label-default">D</span>' +
                '</div>',
            restrict: 'E',
            scope: {
                guide: '='
            }
        };
    });