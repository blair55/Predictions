'use strict';

/**
 * @ngdoc service
 * @name frontendApp.interceptor
 * @description
 * # interceptor
 * Factory in the frontendApp.
 */
angular.module('frontendApp')
  .factory('interceptor', function ($q, notify) {
      return {
          'request': function(config) {

            //var isApiRequest = config.url.indexOf('/api') >= 0;
            
            //if(isApiRequest)
            //{
            //  var newUrl = "http://localhost:55135" + config.url;
            //  config.url = newUrl;
            //}
            
            return config;
          }

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
          ,
          // optional method
         'responseError': function(rejection) {
            // do something on error
            notify.fail(rejection.statusText + " - " + rejection.data);

            // if (canRecover(rejection)) {
            //   return responseOrNewPromise
            // }
            return $q.reject(rejection);
          }
        };
  });

angular.module('frontendApp')
  .config(['$httpProvider', function($httpProvider) {
    $httpProvider.interceptors.push('interceptor');
}]);