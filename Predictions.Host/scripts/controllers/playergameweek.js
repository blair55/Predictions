'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:PlayergameweekCtrl
 * @description
 * # PlayergameweekCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
    .controller('PlayergameweekCtrl', function($scope, $http, $routeParams, title) {
        var url = '/api/playergameweek/' + $routeParams.playerId + '/' + $routeParams.gameWeekNo;
        $http.get(url).success(function(data) {
            $scope.model = data;
            title.set(data.player.name + " / GW#" + data.gameWeekNo);
            title.useBackButton('#/playerprofile/' + data.player.id);
        });
        var neighboursUrl = '/api/gameweekneighbours/' + $routeParams.gameWeekNo;
        $http.get(neighboursUrl).success(function (data) {
            $scope.neighbours = data;
        });
    });