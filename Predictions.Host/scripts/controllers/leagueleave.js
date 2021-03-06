'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:LeagueleaveCtrl
 * @description
 * # LeagueleaveCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
    .controller('LeagueleaveCtrl', function($scope, $http, $routeParams, $location, notify, title) {

        title.set('Leave League');
        var url = '/api/league/' + $routeParams.leagueId;
        $http.get(url).success(function(data) {
            $scope.model = data;
            title.useBackButton('#/league/' + data.id + "/0");
        });

        $scope.submit = function() {
            var postUrl = '/api/leagueleave/' + $scope.model.id;
            $http.post(postUrl).success(function() {
                notify.success("League left successfully");
                $location.path('leagues');
            });
        };
    });