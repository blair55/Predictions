'use strict';

describe('Controller: LeaguedeleteCtrl', function () {

  // load the controller's module
  beforeEach(module('frontendApp'));

  var LeaguedeleteCtrl,
    scope;

  // Initialize the controller and a mock scope
  beforeEach(inject(function ($controller, $rootScope) {
    scope = $rootScope.$new();
    LeaguedeleteCtrl = $controller('LeaguedeleteCtrl', {
      $scope: scope
    });
  }));

  it('should attach a list of awesomeThings to the scope', function () {
    expect(scope.awesomeThings.length).toBe(3);
  });
});
