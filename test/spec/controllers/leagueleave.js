'use strict';

describe('Controller: LeagueleaveCtrl', function () {

  // load the controller's module
  beforeEach(module('frontendApp'));

  var LeagueleaveCtrl,
    scope;

  // Initialize the controller and a mock scope
  beforeEach(inject(function ($controller, $rootScope) {
    scope = $rootScope.$new();
    LeagueleaveCtrl = $controller('LeagueleaveCtrl', {
      $scope: scope
    });
  }));

  it('should attach a list of awesomeThings to the scope', function () {
    expect(scope.awesomeThings.length).toBe(3);
  });
});
