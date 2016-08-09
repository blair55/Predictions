'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:LeagueinviteCtrl
 * @description
 * # LeagueinviteCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
    .controller('LeagueinviteCtrl', function ($scope, $http, $routeParams, title) {
        var url = '/api/leagueinvite/' + $routeParams.leagueId;
        $http.get(url).success(function(data) {
            $scope.model = data;
            title.set('Invite to ' + data.name);
            title.useBackButton('#/league/' + data.id + "/0");
            $scope.model.encodedInviteLink = encodeURIComponent(data.inviteLink);
            $scope.model.encodedShareText = encodeURIComponent("Join my predictions league on Right Result!");
        });
        $scope.share = function() {
            FB.ui({
                method: 'share',
                caption: 'Join my predictions league on Right Result!',
                href: $scope.model.inviteLink
            });
        }
    });