'use strict';

describe('Directive: previousmeetingrow', function () {

  // load the directive's module
  beforeEach(module('frontendApp'));

  var element,
    scope;

  beforeEach(inject(function ($rootScope) {
    scope = $rootScope.$new();
  }));

  it('should make hidden element visible', inject(function ($compile) {
    element = angular.element('<previousmeetingrow></previousmeetingrow>');
    element = $compile(element)(scope);
    expect(element.text()).toBe('this is the previousmeetingrow directive');
  }));
});
