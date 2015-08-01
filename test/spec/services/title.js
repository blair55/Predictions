'use strict';

describe('Service: title', function () {

  // load the service's module
  beforeEach(module('frontendApp'));

  // instantiate service
  var title;
  beforeEach(inject(function (_title_) {
    title = _title_;
  }));

  it('should do something', function () {
    expect(!!title).toBe(true);
  });

});
