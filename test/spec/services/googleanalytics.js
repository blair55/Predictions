'use strict';

describe('Service: googleanalytics', function () {

  // load the service's module
  beforeEach(module('frontendApp'));

  // instantiate service
  var googleanalytics;
  beforeEach(inject(function (_googleanalytics_) {
    googleanalytics = _googleanalytics_;
  }));

  it('should do something', function () {
    expect(!!googleanalytics).toBe(true);
  });

});
