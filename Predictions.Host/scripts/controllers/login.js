'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:LoginCtrl
 * @description
 * # LoginCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
    .controller('LoginCtrl', [
        '$scope', '$location', 'oauthService', function($scope, $location, oauthService) {

            $scope.loginData = {
                userName: "",
                password: ""
            };

            $scope.message = "";

            $scope.login = function() {
                oauthService.login($scope.loginData).then(function(response) {
                        $location.path('/');
                    },
                    function(err) {
                        $scope.message = err.error_description;
                    });
            };
        }
    ]);