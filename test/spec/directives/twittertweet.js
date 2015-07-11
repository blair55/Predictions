'use strict';

describe('Directive: twittertweet', function () {

  // load the directive's module
  beforeEach(module('frontendApp'));

  var element,
    scope;

  beforeEach(inject(function ($rootScope) {
    scope = $rootScope.$new();
  }));

  it('should make hidden element visible', inject(function ($compile) {
    element = angular.element('<twittertweet></twittertweet>');
    element = $compile(element)(scope);
    expect(element.text()).toBe('this is the twittertweet directive');
  }));
});
