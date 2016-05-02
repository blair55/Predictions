'use strict';

describe('Controller: PredictedleaguetableCtrl', function () {

  // load the controller's module
  beforeEach(module('frontendApp'));

  var PredictedleaguetableCtrl,
    scope;

  // Initialize the controller and a mock scope
  beforeEach(inject(function ($controller, $rootScope) {
    scope = $rootScope.$new();
    PredictedleaguetableCtrl = $controller('PredictedleaguetableCtrl', {
      $scope: scope
      // place here mocked dependencies
    });
  }));

  it('should attach a list of awesomeThings to the scope', function () {
    expect(PredictedleaguetableCtrl.awesomeThings.length).toBe(3);
  });
});
