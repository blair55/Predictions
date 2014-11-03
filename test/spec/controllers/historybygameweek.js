'use strict';

describe('Controller: HistorybygameweekCtrl', function () {

  // load the controller's module
  beforeEach(module('frontendApp'));

  var HistorybygameweekCtrl,
    scope;

  // Initialize the controller and a mock scope
  beforeEach(inject(function ($controller, $rootScope) {
    scope = $rootScope.$new();
    HistorybygameweekCtrl = $controller('HistorybygameweekCtrl', {
      $scope: scope
    });
  }));

  it('should attach a list of awesomeThings to the scope', function () {
    expect(scope.awesomeThings.length).toBe(3);
  });
});
