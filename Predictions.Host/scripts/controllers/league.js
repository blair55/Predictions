'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:LeagueCtrl
 * @description
 * # LeagueCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
    .controller('LeagueCtrl', function($scope, $http, $routeParams, auth) {
        var url = '/api/league/' + $routeParams.leagueId;
        $http.get(url).success(function(data) {
            $scope.model = data;
        });
        auth.withPlayer(function (player) {
            $scope.loggedInPlayer = player;
        });
    });