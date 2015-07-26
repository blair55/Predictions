'use strict';

/**
 * @ngdoc directive
 * @name frontendApp.directive:formguide
 * @description
 * # formguide
 */
angular.module('frontendApp')
    .directive('formguide', function() {
        return {
            template: "<ul class='form-guide'>" +
                "<li ng-repeat='g in guide track by $index' class='outcome-{{g}}'><span>{{g}}</span></li>" +
                "</ul>",
            restrict: 'E',
            scope: {
                guide: '='
            }
        };
    });