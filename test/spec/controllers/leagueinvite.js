'use strict';

describe('Controller: LeagueinviteCtrl', function () {

  // load the controller's module
  beforeEach(module('frontendApp'));

  var LeagueinviteCtrl,
    scope;

  // Initialize the controller and a mock scope
  beforeEach(inject(function ($controller, $rootScope) {
    scope = $rootScope.$new();
    LeagueinviteCtrl = $controller('LeagueinviteCtrl', {
      $scope: scope
    });
  }));

  it('should attach a list of awesomeThings to the scope', function () {
    expect(scope.awesomeThings.length).toBe(3);
  });
});
