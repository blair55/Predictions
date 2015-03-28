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
        'ngToast',
        'chart.js',
        'ordinal',
        'angular-loading-bar'
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
        .when('/player/:playerId', {
            templateUrl: 'views/player.html',
            controller: 'PlayerCtrl'
        })
        .when('/playergameweek/:playerId/:gameWeekNo', {
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
        .when('/fixture/:fxid', {
            templateUrl: 'views/fixture.html',
            controller: 'FixtureCtrl'
        })
        .when('/history/month', {
            templateUrl: 'views/historybymonth.html',
            controller: 'HistorybymonthCtrl'
        })
        .when('/history/month/:month', {
            templateUrl: 'views/historybymonthwithmonth.html',
            controller: 'HistorybymonthwithmonthCtrl'
        })
        .when('/history/gameweek', {
            templateUrl: 'views/historybygameweek.html',
            controller: 'HistorybygameweekCtrl'
        })
        .when('/history/gameweek/:gameweekno', {
            templateUrl: 'views/historybygameweekwithgameweek.html',
            controller: 'HistorybygameweekwithgameweekCtrl'
        })
        .when('/admin/addresults/:gameweekno', {
          templateUrl: 'views/adminaddresultsforgameweek.html',
          controller: 'AdminaddresultsforgameweekCtrl'
        })
        .when('/inplay', {
          templateUrl: 'views/inplay.html',
          controller: 'InplayCtrl'
        })
        .when('/login', {
          templateUrl: 'views/login.html',
          controller: 'LoginCtrl'
        })
        .otherwise({
            redirectTo: '/'
        });
    })
    .run(function (oauthService) {
        Chart.defaults.global.responsive = true;
        oauthService.fillAuthData();
    })
    .config(function ($httpProvider, localStorageServiceProvider) {
        $httpProvider.interceptors.push('oauthInterceptorService');
        localStorageServiceProvider.setPrefix('prdlge');
    });