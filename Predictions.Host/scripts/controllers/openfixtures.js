'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:OpenfixturesCtrl
 * @description
 * # OpenfixturesCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
    .controller('OpenfixturesCtrl', function($scope, $http, title) {
        title.set('Open Fixtures');
        title.useBackButton('#/');

        $http.get('/api/openfixtures').success(function(data) {
            $scope.model = data;
        });

        function submitRow(i) {
            var row = $scope.model.rows[i];
            row.submitWithCallBack(row, function() {
                if (i + 1 < $scope.model.rows.length) {
                    submitRow(i + 1);
                } else {
                    $scope.submittingAll = false;
                }
            });
        }

        $scope.anyEditableRows = function() {
            if ($scope.model) {
                for (var i = 0; i < $scope.model.rows.length; i++) {
                    var row = $scope.model.rows[i];
                    if (row.isSubmittable(row)) {
                        return true;
                    }
                }
            }
            return false;
        };

        $scope.$on("doubleDownSet", function(event, args) {
            for (var i = 0; i < $scope.model.rows.length; i++) {
                var row = $scope.model.rows[i];
                if (row.fixture.gameWeekNumber == args.gwno) {
                    row.clearDoubleDown(row);
                }
            }
        });

        $scope.submitAll = function() {
            $scope.submittingAll = true;
            submitRow(0);
        };

    });