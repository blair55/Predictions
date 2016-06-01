'use strict';

describe('Controller: SettimeCtrl', function () {

  // load the controller's module
  beforeEach(module('frontendApp'));

  var SettimeCtrl,
    scope;

  // Initialize the controller and a mock scope
  beforeEach(inject(function ($controller, $rootScope) {
    scope = $rootScope.$new();
    SettimeCtrl = $controller('SettimeCtrl', {
      $scope: scope
      // place here mocked dependencies
    });
  }));

  it('should attach a list of awesomeThings to the scope', function () {
    expect(SettimeCtrl.awesomeThings.length).toBe(3);
  });
});
