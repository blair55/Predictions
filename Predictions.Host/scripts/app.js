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
        .when('/player/:playerId/:leagueId', {
            templateUrl: 'views/player.html',
            controller: 'PlayerCtrl'
        })
        .when('/playergameweek/:playerId/:gameWeekNo', {
            templateUrl: 'views/playergameweek.html',
            controller: 'PlayergameweekCtrl'
        })
        .when('/playerprofile/:playerId', {
            templateUrl: 'views/playerprofile.html',
            controller: 'PlayerprofileCtrl'
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
        .when('/history/:leagueId/month', {
            templateUrl: 'views/historybymonth.html',
            controller: 'HistorybymonthCtrl'
        })
        .when('/history/:leagueId/month/:month', {
            templateUrl: 'views/historybymonthwithmonth.html',
            controller: 'HistorybymonthwithmonthCtrl'
        })
        .when('/history/:leagueId/gameweek', {
            templateUrl: 'views/historybygameweek.html',
            controller: 'HistorybygameweekCtrl'
        })
        .when('/history/:leagueId/gameweek/:gameweekno', {
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
        .when('/leagues', {
          templateUrl: 'views/leagues.html',
          controller: 'LeaguesCtrl'
        })
        .when('/createleague', {
          templateUrl: 'views/createleague.html',
          controller: 'CreateleagueCtrl'
        })
        .when('/league/:leagueId', {
          templateUrl: 'views/league.html',
          controller: 'LeagueCtrl'
        })
        .when('/leagueinvite/:leagueId', {
          templateUrl: 'views/leagueinvite.html',
          controller: 'LeagueinviteCtrl'
        })
        .when('/joinleague/:shareableLeagueId', {
          templateUrl: 'views/leaguejoin.html',
          controller: 'LeaguejoinCtrl'
        })
        .when('/matrix/:leagueId/:gameweekno', {
          templateUrl: 'views/matrix.html',
          controller: 'MatrixCtrl'
        })
        .when('/globalleague/:page', {
          templateUrl: 'views/globalleague.html',
          controller: 'GloballeagueCtrl'
        })
        .when('/leagueleave/:leagueId', {
          templateUrl: 'views/leagueleave.html',
          controller: 'LeagueleaveCtrl'
        })
        .otherwise({
            redirectTo: '/'
        });
    })
    .run(function () {
        Chart.defaults.global.responsive = true;
    })
    .config(function (localStorageServiceProvider) {
        localStorageServiceProvider.setPrefix('prdlge');
    })
	.config(['cfpLoadingBarProvider', function(cfpLoadingBarProvider) {
		cfpLoadingBarProvider.includeSpinner = false;
  }]);