'use strict';

describe('Controller: AdminaddresultsforgameweekCtrl', function () {

  // load the controller's module
  beforeEach(module('frontendApp'));

  var AdminaddresultsforgameweekCtrl,
    scope;

  // Initialize the controller and a mock scope
  beforeEach(inject(function ($controller, $rootScope) {
    scope = $rootScope.$new();
    AdminaddresultsforgameweekCtrl = $controller('AdminaddresultsforgameweekCtrl', {
      $scope: scope
    });
  }));

  it('should attach a list of awesomeThings to the scope', function () {
    expect(scope.awesomeThings.length).toBe(3);
  });
});
