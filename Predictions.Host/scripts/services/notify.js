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
          var content = "<span class='glyphicon glyphicon-ok'></span> " + msg;
          ngToast.create({ class: 'success', content: content });
      };

      var fail = function (msg) {
          var content = "<span class='glyphicon glyphicon-ban-circle'></span> " + msg;
          ngToast.create({ class: 'danger', content: content });
      };

      return {
          success: success,
          fail: fail
      };
  });