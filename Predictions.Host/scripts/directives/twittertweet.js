'use strict';

/**
 * @ngdoc directive
 * @name frontendApp.directive:twittertweet
 * @description
 * # twittertweet
 */
angular.module('frontendApp')
    .directive('tweet', [
        '$window', '$location',
        function($window, $location) {
            return {
                restrict: 'A',
                scope: {
                    tweet: '=',
                    tweetUrl: '='
                },
                link: function(scope, element, attrs) {
                    renderTweetButton();
                    var watchAdded = false;

                    function renderTweetButton() {
                        if (!scope.tweet && !watchAdded) {
                            watchAdded = true;
                            var unbindWatch = scope.$watch('tweet', function(newValue, oldValue) {
                                if (newValue) {
                                    renderTweetButton();
                                    unbindWatch();
                                }
                            });
                            return;
                        } else {
                            setTimeout(function() {
                                element.html('<a href="https://twitter.com/share" class="twitter-share-button" data-text="' + scope.tweet + '" data-url="' + (scope.tweetUrl || $location.absUrl()) + '">Tweet</a>');
                                $window.twttr.widgets.load(element.parent()[0]);
                            }, 100);
                        }
                    }
                }
            };
        }
    ]);