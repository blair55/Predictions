'use strict';

describe('Controller: LeagueCtrl', function () {

  // load the controller's module
  beforeEach(module('frontendApp'));

  var LeagueCtrl,
    scope;

  // Initialize the controller and a mock scope
  beforeEach(inject(function ($controller, $rootScope) {
    scope = $rootScope.$new();
    LeagueCtrl = $controller('LeagueCtrl', {
      $scope: scope
    });
  }));

  it('should attach a list of awesomeThings to the scope', function () {
    expect(scope.awesomeThings.length).toBe(3);
  });
});
