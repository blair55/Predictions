'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:PlayerprofileCtrl
 * @description
 * # PlayerprofileCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
    .controller('PlayerprofileCtrl', function($scope, $http, $routeParams, title) {

        var url = '/api/playerprofile/' + $routeParams.playerId;
        $http.get(url).success(function(data) {
            $scope.model = data;
            title.set(data.player.name);
            title.useBackButton('#/');
        });

        var graphUrl = '/api/playerprofilegraph/' + $routeParams.playerId;
        $http.get(graphUrl).success(function(data) {
            $scope.labels = data.labels;
            $scope.data = data.data;
            $scope.options = {
                animationSteps: 10,
                //showScale: !window.mobilecheck(),
                showScale: false,
                scaleOverride: false,
                // ** Required if scaleOverride is true **
                // Number - The number of steps in a hard coded scale
                //scaleSteps: 16,
                //// Number - The value jump in the hard coded scale
                //scaleStepWidth: 1,
                //// Number - The scale starting value
                //scaleStartValue: 0,
            };
        });
    });