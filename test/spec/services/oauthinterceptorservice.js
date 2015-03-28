'use strict';

describe('Service: oauthInterceptorService', function () {

  // load the service's module
  beforeEach(module('frontendApp'));

  // instantiate service
  var oauthInterceptorService;
  beforeEach(inject(function (_oauthInterceptorService_) {
    oauthInterceptorService = _oauthInterceptorService_;
  }));

  it('should do something', function () {
    expect(!!oauthInterceptorService).toBe(true);
  });

});
