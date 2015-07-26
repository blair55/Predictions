'use strict';

/**
 * @ngdoc service
 * @name frontendApp.googleanalytics
 * @description
 * # googleanalytics
 * Service in the frontendApp.
 */
angular.module('frontendApp')
    .service('googleanalytics', function googleanalytics($rootScope, $window, $location) {
        var init = function() {
            if ($window.ga) {
                $rootScope.$on('$routeChangeSuccess', function() {
                    $window.ga('send', 'pageview', { page: $location.url() });
                });
            }
        }
        return { init: init };
    });