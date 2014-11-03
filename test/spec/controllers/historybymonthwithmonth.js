'use strict';

describe('Controller: HistorybymonthwithmonthCtrl', function () {

  // load the controller's module
  beforeEach(module('frontendApp'));

  var HistorybymonthwithmonthCtrl,
    scope;

  // Initialize the controller and a mock scope
  beforeEach(inject(function ($controller, $rootScope) {
    scope = $rootScope.$new();
    HistorybymonthwithmonthCtrl = $controller('HistorybymonthwithmonthCtrl', {
      $scope: scope
    });
  }));

  it('should attach a list of awesomeThings to the scope', function () {
    expect(scope.awesomeThings.length).toBe(3);
  });
});
