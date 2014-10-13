'use strict';

/**
 * @ngdoc overview
 * @name frontendApp
 * @description
 * # frontendApp
 *
 * Main module of the application.
 */
angular
  .module('frontendApp', [
    'ngAnimate',
    'ngCookies',
    'ngResource',
    'ngRoute',
    'ngSanitize',
    'ngTouch',
    'LocalStorageModule',
    'ui.bootstrap',
    'ui.bootstrap.datetimepicker',
  ])
  .config(function ($routeProvider) {
    $routeProvider
      .when('/', {
        templateUrl: 'views/main.html',
        controller: 'MainCtrl'
      })
      .when('/leaguetable', {
        templateUrl: 'views/leaguetable.html',
        controller: 'LeaguetableCtrl'
      })
      .when('/player/:playerName', {
        templateUrl: 'views/player.html',
        controller: 'PlayerCtrl'
      })
      .when('/playergameweek/:playerName/:gameWeekNo', {
        templateUrl: 'views/playergameweek.html',
        controller: 'PlayergameweekCtrl'
      })
      .when('/openfixtures', {
        templateUrl: 'views/openfixtures.html',
        controller: 'OpenfixturesCtrl'
      })
      .when('/admin/addgameweek', {
        templateUrl: 'views/adminaddgameweek.html',
        controller: 'AdminaddgameweekCtrl'
      })
      .when('/admin/addresults', {
        templateUrl: 'views/adminaddresults.html',
        controller: 'AdminaddresultsCtrl'
      })
      .when('/pastgameweeks', {
        templateUrl: 'views/pastgameweeks.html',
        controller: 'PastgameweeksCtrl'
      })
      .when('/gameweekscores/:gameWeekNo', {
        templateUrl: 'views/gameweekscores.html',
        controller: 'GameweekscoresCtrl'
      })
      .when('/fixture/:fxid', {
        templateUrl: 'views/fixture.html',
        controller: 'FixtureCtrl'
      })
      .otherwise({
        redirectTo: '/'
      });
  });