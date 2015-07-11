'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:AdminaddgameweekCtrl
 * @description
 * # AdminaddgameweekCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
    .controller('AdminaddgameweekCtrl', function($scope, $http, $location, notify) {

        var url = '/api/admin/gameweek';

        $http.get(url).success(function(data) {
            $scope.model = data;
        });

        $scope.submit = function() {
            $http.post(url).success(function() {
                notify.success("gameweek added successfully");
                $location.path('openfixtures');
            });
        };
    });