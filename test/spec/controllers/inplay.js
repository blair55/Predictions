'use strict';

describe('Controller: InplayCtrl', function () {

  // load the controller's module
  beforeEach(module('frontendApp'));

  var InplayCtrl,
    scope;

  // Initialize the controller and a mock scope
  beforeEach(inject(function ($controller, $rootScope) {
    scope = $rootScope.$new();
    InplayCtrl = $controller('InplayCtrl', {
      $scope: scope
    });
  }));

  it('should attach a list of awesomeThings to the scope', function () {
    expect(scope.awesomeThings.length).toBe(3);
  });
});
