'use strict';

/**
 * @ngdoc directive
 * @name frontendApp.directive:facebooklike
 * @description
 * # facebooklike
 */
angular.module('frontendApp')
    .directive('facebooklike', [
        '$window', function($window) {
            return {
                restrict: 'A',
                scope: {
                    facebooklike: '=?'
                },
                link: function(scope, element, attrs) {
                    renderLikeButton();
                    var watchAdded = false;

                    function renderLikeButton() {
                        if (!!attrs.fbLike && !scope.fbLike && !watchAdded) {
                            watchAdded = true;
                            var unbindWatch = scope.$watch('facebooklike', function (newValue, oldValue) {
                                if (newValue) {
                                    renderLikeButton();
                                    unbindWatch();
                                }
                            });
                            return;
                        } else {
                            setTimeout(function() {
                                element.html('<div class="fb-like"' + (!!scope.fbLike ? ' data-href="' + scope.fbLike + '"' : '') + ' data-layout="button_count" data-action="like" data-show-faces="true" data-share="false"></div>');
                                $window.FB.XFBML.parse(element.parent()[0]);
                            }, 100);
                        }
                    }
                }
            };
        }
    ]);