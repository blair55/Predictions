'use strict';

describe('Controller: HistorybymonthCtrl', function () {

  // load the controller's module
  beforeEach(module('frontendApp'));

  var HistorybymonthCtrl,
    scope;

  // Initialize the controller and a mock scope
  beforeEach(inject(function ($controller, $rootScope) {
    scope = $rootScope.$new();
    HistorybymonthCtrl = $controller('HistorybymonthCtrl', {
      $scope: scope
    });
  }));

  it('should attach a list of awesomeThings to the scope', function () {
    expect(scope.awesomeThings.length).toBe(3);
  });
});
