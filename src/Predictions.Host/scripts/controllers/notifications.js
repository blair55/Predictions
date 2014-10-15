'use strict';

/**
 * @ngdoc function
 * @name frontendApp.controller:NotificationsCtrl
 * @description
 * # NotificationsCtrl
 * Controller of the frontendApp
 */
angular.module('frontendApp')
  .controller('NotificationsCtrl', function ($scope, notify) {
	$scope.alerts = notify.notifications;
	$scope.closeAlert = function(index) {
		notify.close(index);
	};
});
