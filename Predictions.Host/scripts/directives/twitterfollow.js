'use strict';

/**
 * @ngdoc directive
 * @name frontendApp.directive:twitterfollow
 * @description
 * # twitterfollow
 */
angular.module('frontendApp')
    .directive('twitterfollow', [
        '$window',
        function($window) {
            return {
                restrict: 'A',
                link: function(scope, element, attrs) {
                    setTimeout(function() {
                        element.html('<a href="https://twitter.com/rightresu_lt" class="twitter-follow-button">Follow @rightresu_lt</a>');
                        $window.twttr.widgets.load(element.parent()[0]);
                    }, 100);
                }
            };
        }
    ]);