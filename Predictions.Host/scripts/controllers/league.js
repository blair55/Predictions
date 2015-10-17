'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:LeagueCtrl
 * @description
 * # LeagueCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
    .controller('LeagueCtrl', function ($scope, $http, $routeParams, auth, title) {
        var url = '/api/league/' + $routeParams.leagueId + '/' + $routeParams.page;
        $http.get(url).success(function(data) {
            $scope.model = data;
            title.set(data.name);
            title.useBackButton('#/leagues');
            auth.withPlayer(function (player) {
                $scope.loggedInPlayer = player;
                $scope.isLoggedInPlayerLeagueAdmin = player.id == data.adminId;
            });
        });
    });