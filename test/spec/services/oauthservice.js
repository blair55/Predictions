'use strict';

describe('Service: oauthService', function () {

  // load the service's module
  beforeEach(module('frontendApp'));

  // instantiate service
  var oauthService;
  beforeEach(inject(function (_oauthService_) {
    oauthService = _oauthService_;
  }));

  it('should do something', function () {
    expect(!!oauthService).toBe(true);
  });

});
