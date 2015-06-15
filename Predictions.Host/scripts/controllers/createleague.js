'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:CreateleagueCtrl
 * @description
 * # CreateleagueCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
    .controller('CreateleagueCtrl', function($scope, $http, $location, notify) {

        $scope.submit = function() {
            var league = { name: $scope.leagueName };

            $http.post("/api/createleague", league).success(function(data) {
                notify.success(data.name + " created successfully");
                $location.path('leagueinvite/' + data.id);
            });
        };
    });