'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:LeaguejoinCtrl
 * @description
 * # LeaguejoinCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
    .controller('LeaguejoinCtrl', function($scope, $http, $routeParams, $location, notify, title) {

        title.set('Join League');
        var url = '/api/leaguejoin/' + $routeParams.shareableLeagueId;
        $http.get(url).success(function(data) {
            $scope.model = data;
        });

        $scope.submit = function() {
            var postUrl = '/api/leaguejoin/' + $scope.model.id;
            $http.post(postUrl).success(function(data) {
                notify.success(data.name + " joined successfully");
                $location.path('league/' + data.id);
            });
        };
    });