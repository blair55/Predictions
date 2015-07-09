'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:LeagueinviteCtrl
 * @description
 * # LeagueinviteCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
    .controller('LeagueinviteCtrl', function($scope, $http, $routeParams) {
        var url = '/api/leagueinvite/' + $routeParams.leagueId;
        $http.get(url).success(function(data) {
            $scope.model = data;
            $scope.model.encodedInviteLink = encodeURIComponent(data.inviteLink);
            $scope.model.encodedShareText = encodeURIComponent("Join my Predctions League!");
        });
        $scope.share = function() {
        FB.ui({
            method: 'share',
            href:  $scope.model.inviteLink
        });
    }
});