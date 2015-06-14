'use strict';

describe('Controller: CreateleagueCtrl', function () {

  // load the controller's module
  beforeEach(module('frontendApp'));

  var CreateleagueCtrl,
    scope;

  // Initialize the controller and a mock scope
  beforeEach(inject(function ($controller, $rootScope) {
    scope = $rootScope.$new();
    CreateleagueCtrl = $controller('CreateleagueCtrl', {
      $scope: scope
    });
  }));

  it('should attach a list of awesomeThings to the scope', function () {
    expect(scope.awesomeThings.length).toBe(3);
  });
});
