'use strict';

describe('Controller: GlobalhistorybygameweekwithgameweekCtrl', function () {

  // load the controller's module
  beforeEach(module('frontendApp'));

  var GlobalhistorybygameweekwithgameweekCtrl,
    scope;

  // Initialize the controller and a mock scope
  beforeEach(inject(function ($controller, $rootScope) {
    scope = $rootScope.$new();
    GlobalhistorybygameweekwithgameweekCtrl = $controller('GlobalhistorybygameweekwithgameweekCtrl', {
      $scope: scope
    });
  }));

  it('should attach a list of awesomeThings to the scope', function () {
    expect(scope.awesomeThings.length).toBe(3);
  });
});
