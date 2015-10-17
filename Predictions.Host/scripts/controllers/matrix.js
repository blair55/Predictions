'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:MatrixCtrl
 * @description
 * # MatrixCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
    .controller('MatrixCtrl', function ($scope, $http, $routeParams, auth, title) {
        var url = '/api/gameweekmatrix/' + $routeParams.leagueId + '/gameweek/' + $routeParams.gameweekno + '/page/' + $routeParams.page;
        $http.get(url).success(function(data) {
            $scope.model = data;
            title.set(data.league.name + " / GW#" + data.gameWeekNo + " Matrix");
            title.useBackButton('#/league/' + data.league.id + "/0");
        });
        //var neighboursUrl = '/api/gameweekneighbours/' + $routeParams.gameweekno;
        //$http.get(neighboursUrl).success(function (data) {
        //    $scope.neighbours = data;
        //});
        auth.withPlayer(function (player) {
            $scope.loggedInPlayer = player;
        });
    });
