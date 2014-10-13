'use strict';

/**
 * @ngdoc directive
 * @name frontendApp.directive:submitscore
 * @description
 * # submitscore
 */
angular.module('frontendApp')
	.directive('submitscore', function () {
    return {
		templateUrl: 'views/submitscore.html'
		,restrict: 'E'
		// ,link: function postLink(scope, element, attrs) {
		// 	//element.text('this is the submitScore directive');
		// 	// console.log(scope);
			
		// }
		// ,scope: {
		//     fixture: '=row'
		// }
    };
});
