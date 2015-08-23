'use strict';

describe('Controller: GlobalhistorybymonthCtrl', function () {

  // load the controller's module
  beforeEach(module('frontendApp'));

  var GlobalhistorybymonthCtrl,
    scope;

  // Initialize the controller and a mock scope
  beforeEach(inject(function ($controller, $rootScope) {
    scope = $rootScope.$new();
    GlobalhistorybymonthCtrl = $controller('GlobalhistorybymonthCtrl', {
      $scope: scope
    });
  }));

  it('should attach a list of awesomeThings to the scope', function () {
    expect(scope.awesomeThings.length).toBe(3);
  });
});
