'use strict';

describe('Controller: PlayerprofileCtrl', function () {

  // load the controller's module
  beforeEach(module('frontendApp'));

  var PlayerprofileCtrl,
    scope;

  // Initialize the controller and a mock scope
  beforeEach(inject(function ($controller, $rootScope) {
    scope = $rootScope.$new();
    PlayerprofileCtrl = $controller('PlayerprofileCtrl', {
      $scope: scope
    });
  }));

  it('should attach a list of awesomeThings to the scope', function () {
    expect(scope.awesomeThings.length).toBe(3);
  });
});
