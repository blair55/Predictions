'use strict';

/**
 * @ngdoc service
 * @name frontendApp.oauthInterceptorService
 * @description
 * # oauthInterceptorService
 * Factory in the frontendApp.
 */
angular.module('frontendApp')
    .factory('oauthInterceptorService', [
        '$q', '$location', 'localStorageService', function($q, $location, localStorageService) {

            var _request = function(config) {

                config.headers = config.headers || {};

                var authData = localStorageService.get('authorizationData');
                if (authData) {
                    config.headers.Authorization = 'Bearer ' + authData.token;
                }

                return config;
            }

            var _responseError = function(rejection) {
                if (rejection.status === 401) {
                    $location.path('/login');
                }
                return $q.reject(rejection);
            }

            return {
                request: _request,
                responseError: _responseError
            }
        }
    ]);