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
            $rootScope.useBackButton = false;
            $rootScope.backButtonUrl = "";
        });
        return {
            set: function(newTitle) {
                $rootScope.title = newTitle;
            },
            useBackButton: function (url) {
                $rootScope.useBackButton = true;
                $rootScope.backButtonUrl = url;
            }
        };
    });