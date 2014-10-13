'use strict';

/**
 * @ngdoc service
 * @name frontendApp.notify
 * @description
 * # notify
 * Service in the frontendApp.
 */
angular.module('frontendApp')
  .service('notify', function notify() {
    
	var notifications = [];

	var success = function(msg){
		notifications.push({ type: 'success', msg: msg });
	};

	var fail = function(msg){
		notifications.push({ type: 'danger', msg: msg });
	};

	var close = function(index){
		notifications.splice(index, 1);
	};

	return {
		notifications: notifications,
		success: success,
		fail: fail,
		close: close
	};
});
