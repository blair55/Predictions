'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:EditpredictionsCtrl
 * @description
 * # EditpredictionsCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
    .controller('EditpredictionsCtrl', function ($scope, $http) {

    $http.get('/api/editpredictions').success(function (data) {
        $scope.model = data;
    });

    $scope.submitResult = function (row, index) {
        var prediction = {
            predictionId: row.predictionId,
            score: score
        };
        $http.post('/api/editprediction', prediction).success(function (data) {
            row.submitted = true;
        });
    };
});
