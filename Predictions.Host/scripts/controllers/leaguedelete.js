'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:LeaguedeleteCtrl
 * @description
 * # LeaguedeleteCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
    .controller('LeaguedeleteCtrl', function($scope, $http, $routeParams, $location, notify, title) {

        var url = '/api/league/' + $routeParams.leagueId;
        $http.get(url).success(function(data) {
            $scope.model = data;
            title.set('Delete League');
            title.useBackButton('#/league/' + data.id + "/0");
        });

        $scope.submit = function() {
            var postUrl = '/api/leaguedelete/' + $scope.model.id;
            $http.post(postUrl).success(function() {
                notify.success("League deleted successfully");
                $location.path('leagues');
            });
        };
    });