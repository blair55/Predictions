'use strict';

/**
* @ngdoc function
* @name frontendApp.controller:MainCtrl
* @description
* # MainCtrl
* Controller of the frontendApp
*/
angular.module('frontendApp')
.controller('MainCtrl', function($scope, $http, localStorageService, auth, title) {
    title.set('Right Result');
    auth.withPlayer(function(player) {
        $scope.loggedInPlayer = player;
    });
    $http.get('/api/getopenfixtureswithnopredictionsforplayercount').success(function(d1) {
        $scope.openfixturecount = d1;
        $http.get('/api/getleaguepositionforplayer').success(function(d2) {
            $scope.global = d2;
            $http.get('/api/getlastgameweekandwinner').success(function(d3) {
                $scope.lastgw = d3;
                $http.get('/api/leagues').success(function(d4) {
                    $scope.leagues = d4;
                });
            });
        });
    });
    // $scope.hasAckedHomeScreenMsg = localStorageService.get('hasAckedHomeScreenMsg-2');
    // $scope.onAckHomeScreenMsg = function () {
        //     localStorageService.set('hasAckedHomeScreenMsg-2', true);
        //     $scope.hasAckedHomeScreenMsg = true;
        // };
    });
    