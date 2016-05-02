'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:PredictedleaguetableCtrl
 * @description
 * # PredictedleaguetableCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
    .controller('PredictedleaguetableCtrl', function($scope, $http, title) {
        var url = '/api/predictedleaguetable';
        $http.get(url).success(function(data) {
            $scope.model = data;
            title.set("Predicted League Table");
            title.useBackButton('#/');
        });
    });
