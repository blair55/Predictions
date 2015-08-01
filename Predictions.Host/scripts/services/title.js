'use strict';

/**
 * @ngdoc service
 * @name frontendApp.title
 * @description
 * # title
 * Service in the frontendApp.
 */
angular.module('frontendApp')
    .service('title', function($rootScope) {
        $rootScope.$on("$routeChangeSuccess", function(currentRoute, previousRoute) {
            $rootScope.title = "";
        });
        return {
            set: function(newTitle) {
                $rootScope.title = newTitle;
            }
        };
    });