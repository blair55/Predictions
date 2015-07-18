'use strict';

/**
 * @ngdoc directive
 * @name frontendApp.directive:twittertweet
 * @description
 * # twittertweet
 */
angular.module('frontendApp')
    .directive('twittertweet', [
        '$window', '$location',
        function($window, $location) {
            return {
                restrict: 'A',
                scope: {
                    twittertweet: '=',
                    tweetUrl: '='
                },
                link: function(scope, element, attrs) {
                    renderTweetButton();
                    var watchAdded = false;

                    function renderTweetButton() {
                        if (!scope.twittertweet && !watchAdded) {
                            watchAdded = true;
                            var unbindWatch = scope.$watch('twittertweet', function(newValue, oldValue) {
                                if (newValue) {
                                    renderTweetButton();
                                    unbindWatch();
                                }
                            });
                            return;
                        } else {
                            setTimeout(function() {
                                element.html('<a href="https://twitter.com/share" class="twitter-share-button" data-text="' + scope.twittertweet + '" data-url="' + (scope.tweetUrl || $location.absUrl()) + '">Tweet</a>');
                                $window.twttr.widgets.load(element.parent()[0]);
                            }, 100);
                        }
                    }
                }
            };
        }
    ]);