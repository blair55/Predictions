'use strict';

/**
 * @ngdoc service
 * @name frontendApp.auth
 * @description
 * # auth
 * Service in the frontendApp.
 */
angular.module('frontendApp')
  .service('auth', function auth($http) {
  	var withPlayer = function(callback){
		$http.get('/api/whoami').success(function(player){
			callback(player);
		});
  	};

  	return { withPlayer : withPlayer };
  });
