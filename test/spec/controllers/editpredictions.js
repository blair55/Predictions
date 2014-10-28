'use strict';

describe('Controller: EditpredictionsCtrl', function () {

  // load the controller's module
  beforeEach(module('frontendApp'));

  var EditpredictionsCtrl,
    scope;

  // Initialize the controller and a mock scope
  beforeEach(inject(function ($controller, $rootScope) {
    scope = $rootScope.$new();
    EditpredictionsCtrl = $controller('EditpredictionsCtrl', {
      $scope: scope
    });
  }));

  it('should attach a list of awesomeThings to the scope', function () {
    expect(scope.awesomeThings.length).toBe(3);
  });
});
