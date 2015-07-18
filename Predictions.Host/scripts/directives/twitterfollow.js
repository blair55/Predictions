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
                        element.html('<a href="https://twitter.com/predictionslge" class="twitter-follow-button">Follow @PredictionsLge</a>');
                        $window.twttr.widgets.load(element.parent()[0]);
                    }, 100);
                }
            };
        }
    ]);