'use strict';

/**
 * @ngdoc service
 * @name frontendApp.notify
 * @description
 * # notify
 * Service in the frontendApp.
 */
angular.module('frontendApp')
  .service('notify', function notify(ngToast) {

      var success = function (msg) {
          ngToast.create({ class: 'success', content: msg });
      };

      var fail = function (msg) {
          ngToast.create({ class: 'danger', content: msg });
      };

      return {
          success: success,
          fail: fail
      };
  });