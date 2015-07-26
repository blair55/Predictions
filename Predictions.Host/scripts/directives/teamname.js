'use strict';

/**
 * @ngdoc directive
 * @name frontendApp.directive:teamname
 * @description
 * # teamname
 */
angular.module('frontendApp')
    .directive('teamname', function() {
        return {
            template:
                '<span class="team-name-full">{{team.full}}</span>' +
                    '<span class="team-name-abrv">{{team.abrv}}</span>',
            restrict: 'E',
            scope: {
                team: '='
            }
        };
    });