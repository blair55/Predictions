'use strict';

describe('Controller: GlobalhistorybygameweekCtrl', function () {

  // load the controller's module
  beforeEach(module('frontendApp'));

  var GlobalhistorybygameweekCtrl,
    scope;

  // Initialize the controller and a mock scope
  beforeEach(inject(function ($controller, $rootScope) {
    scope = $rootScope.$new();
    GlobalhistorybygameweekCtrl = $controller('GlobalhistorybygameweekCtrl', {
      $scope: scope
    });
  }));

  it('should attach a list of awesomeThings to the scope', function () {
    expect(scope.awesomeThings.length).toBe(3);
  });
});
