'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:CreateleagueCtrl
 * @description
 * # CreateleagueCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
    .controller('CreateleagueCtrl', function($scope, $http, notify) {

        $scope.submit = function() {
            var league = { name: $scope.leagueName };

            $http.post("/api/createleague", league).success(function(data) {
                notify.success("league created successfully");
            });
        };
    });