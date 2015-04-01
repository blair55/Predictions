'use strict';

/**
 * @ngdoc service
 * @name frontendApp.interceptor
 * @description
 * # interceptor
 * Factory in the frontendApp.
 */
angular.module('frontendApp')
    .factory('interceptor', function ($q, $location, notify) {
        return {
            //'request': function(config) {
            //    return config;
            //}

            //  // optional method
            // 'requestError': function(rejection) {
            //    // do something on error
            //    if (canRecover(rejection)) {
            //      return responseOrNewPromise
            //    }
            //    return $q.reject(rejection);
            //  },

            //  // optional method
            //  'response': function(response) {
            //    // do something on success
            //    return response;
            //  },
            'responseError': function(rejection) {
                // do something on error

                //if (rejection.status === 401) {
                //    $location.path('/login');
                //}
                //else {
                    var msg = rejection.statusText + " - " + rejection.data;
                    notify.fail(msg);
                //}
                
                // if (canRecover(rejection)) {
                //   return responseOrNewPromise
                // }
                return $q.reject(rejection);
            }
        };
    });

angular.module('frontendApp')
    .config([
        '$httpProvider', function($httpProvider) {
            $httpProvider.interceptors.push('interceptor');
        }
    ]);
