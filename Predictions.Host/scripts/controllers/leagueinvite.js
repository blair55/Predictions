'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:LeagueinviteCtrl
 * @description
 * # LeagueinviteCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
    .controller('LeagueinviteCtrl', function($scope, $http, $routeParams) {
        var url = '/api/leagueinvite/' + $routeParams.leagueId;
        $http.get(url).success(function(data) {
            $scope.model = data;
        });
    });