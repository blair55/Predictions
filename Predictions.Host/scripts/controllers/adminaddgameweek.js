'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:AdminaddgameweekCtrl
 * @description
 * # AdminaddgameweekCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
    .controller('AdminaddgameweekCtrl', function ($scope, $http, $location, $modal, localStorageService, notify) {

        $scope.teams = ["Arsenal", "Aston Villa", "Burnley", "Chelsea", "Crystal Palace", "Everton",
                        "Hull", "Leicester", "Liverpool", "Man City", "Man Utd",
                        "Manchester United", "Newcastle", "QPR", "Southampton", "Stoke",
                        "Sunderland", "Swansea", "Tottenham", "West Brom", "West Ham"];

        $scope.gameweek = {
            fixtures: [
                { home: "", away: "", kickoff: "" },
                { home: "", away: "", kickoff: "" },
                { home: "", away: "", kickoff: "" }
            ]
        };

        $scope.addFixture = function () {
            var latestFixture = $scope.gameweek.fixtures[$scope.gameweek.fixtures.length - 1];
            var newKickOff = angular.copy(latestFixture.kickoff);
            $scope.gameweek.fixtures.push({ home: "", away: "", kickoff: newKickOff });
        }

        $scope.removeFixture = function (index) {
            $scope.gameweek.fixtures.splice(index, 1);
        }

        $scope.submit = function () {
            $scope.inSubmission = true;
            $http.post('/api/admin/gameweek', $scope.gameweek).success(function (data) {
                $scope.inSubmission = false;
                notify.success('gameweek added');
                $location.path('openfixtures');
            }).error(function (data, status, headers, config) {
                $scope.inSubmission = false;
            });
        };

        $scope.open = function (index, fixture) {
            
            var modalInstance = $modal.open({
                templateUrl: 'addfixturemodal.html',
                controller: 'AddfixturemodalCtrl',
                size: 'sm',
                resolve: {
                    index: function () { return index; },
                    fixture: function () { return fixture; }
                }
            });

            modalInstance.result.then(function (data) {
                $scope.gameweek.fixtures[data.index].kickoff = data.kickoff;
            }, function () {
                //$log.info('Modal dismissed at: ' + new Date());
            });
        };

    });


angular.module('frontendApp')
    .controller('AddfixturemodalCtrl', function ($scope, $modalInstance, index, fixture) {

        $scope.fixture = fixture;
        var i = index;

        $scope.onTimeSet = function (oldTime, newTime) {
            $modalInstance.close({ index: i, kickoff: newTime });
        };
    });