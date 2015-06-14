'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:LeaguesCtrl
 * @description
 * # LeaguesCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
    .controller('LeaguesCtrl', function($scope, $http) {
        $http.get('/api/leagues').success(function(data) {
            $scope.model = data;
        });
    });