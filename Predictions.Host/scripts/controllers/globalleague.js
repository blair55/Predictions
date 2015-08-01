'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:GloballeagueCtrl
 * @description
 * # GloballeagueCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
    .controller('GloballeagueCtrl', function ($scope, $http, $routeParams, auth, title) {
        var url = '/api/globalleague/' + $routeParams.page;
        $http.get(url).success(function(data) {
            $scope.model = data;
            title.set('Global League Table');
        });
        auth.withPlayer(function (player) {
            $scope.loggedInPlayer = player;
        });
    });