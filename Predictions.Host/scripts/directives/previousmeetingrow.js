'use strict';

/**
 * @ngdoc directive
 * @name frontendApp.directive:previousmeetingrow
 * @description
 * # previousmeetingrow
 */
angular.module('frontendApp')
  .directive('previousmeetingrow', function () {
      return {
          //templateUrl: 'views/directives/previousmeetingrow.html',
          template:
              '<td><span class="label label-primary">{{ row.kickoff | date:"MMM yyyy" }}</span></td>' +
              '<td>' +
                  '<span class="team-name">{{row.homeTeamName}}</span>' +
              '</td>' +
              '<td>{{row.homeTeamScore}}</td>' +
              '<td>-</td>' +
              '<td>{{row.awayTeamScore}}</td>' +
              '<td>' +
                  '<span class="team-name">{{row.awayTeamName}}</span>' +
              '</td>',
          restrict: 'EA',
          scope: {
              row: '=previousmeetingrow'
          }
      };
  });
