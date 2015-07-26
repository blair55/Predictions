'use strict';

/**
 * @ngdoc directive
 * @name frontendApp.directive:doubledownbutton
 * @description
 * # doubledownbutton
 */
angular.module('frontendApp')
    .directive('doubledownbutton', function($timeout, $http, notify) {
        return {
            template:
                '<div class="dd" ng-class="ddClass">' +
                    '<span class="label dd-button" ng-click="trySetDoubleDown(row)">' +
                    '<i class="glyphicon glyphicon-ok"></i>&nbsp;Double Down</span>' +
                    '</div>',
            restrict: 'E',
            scope: {
                row: '='
            },
            link: function (scope) {
                var setDdClass = function (row) {
                    if (scope.row.state == "create") {
                        scope.ddClass = "dd-unselectable";
                    } else {
                        scope.ddClass = scope.row.isDoubleDown ? "dd-selected" : "dd-selectable";
                    }
                }

                scope.trySetDoubleDown = function(row) {
                    if (row.state == "create") {
                        notify.fail("Submit prediction before using Double Down");
                    } else {
                        if (!row.isDoubleDown) {
                            var doubleDownUrl = "/api/doubledown/" + row.predictionId;
                            $http.post(doubleDownUrl).success(function () {
                                notify.success("Double Down on " + row.fixture.home.full + " v " + row.fixture.away.full);
                                scope.$emit("doubleDownSet", {
                                    gwno: row.fixture.gameWeekNumber
                                });
                                $timeout(function () {
                                    row.isDoubleDown = true;
                                    setDdClass(row);
                                }, 10);
                            });
                        }
                    }
                }

                scope.row.clearDoubleDown = function (row) {
                    row.isDoubleDown = false;
                    setDdClass(row);
                };

                setDdClass(scope.row);
            }
        };
    });