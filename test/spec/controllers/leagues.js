'use strict';

describe('Controller: LeaguesCtrl', function () {

  // load the controller's module
  beforeEach(module('frontendApp'));

  var LeaguesCtrl,
    scope;

  // Initialize the controller and a mock scope
  beforeEach(inject(function ($controller, $rootScope) {
    scope = $rootScope.$new();
    LeaguesCtrl = $controller('LeaguesCtrl', {
      $scope: scope
    });
  }));

  it('should attach a list of awesomeThings to the scope', function () {
    expect(scope.awesomeThings.length).toBe(3);
  });
});
