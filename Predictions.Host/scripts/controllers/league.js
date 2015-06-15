'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:LeagueCtrl
 * @description
 * # LeagueCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
    .controller('LeagueCtrl', function($scope, $http, $routeParams) {
        var url = '/api/league/' + $routeParams.leagueId;
        $http.get(url).success(function(data) {
            $scope.model = data;
        });
    });