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
              '<td class="cell-15"><span class="label label-primary">{{ row.kickoff | date:"MMM yyyy" }}</span></td>' +
              '<td class="cell-25 text-right">' +
                  '<span class="text-uppercase">' +
                    '<teamname team="row.home"></teamname>' +
                  '</span>' +
              '</td>' +
              '<td class="cell-20 text-center">{{row.homeTeamScore}} - {{row.awayTeamScore}}</td>' +
              '<td class="cell-40">' +
                  '<span class="text-uppercase">' +
                    '<teamname team="row.away"></teamname>' +
                  '</span>' +
              '</td>',
          restrict: 'EA',
          scope: {
              row: '=previousmeetingrow'
          }
      };
  });
