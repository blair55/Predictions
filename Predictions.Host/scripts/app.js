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
        'ngToast',
        'chart.js',
        'ordinal',
        'angular-loading-bar',
        'blockUI'
    ])
    .config(function($routeProvider) {
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

            //.when('/history/global/gameweek', {
            //    templateUrl: 'views/globalhistorybygameweek.html',
            //    controller: 'GlobalhistorybygameweekCtrl'
            //})
            //.when('/history/global/gameweek/:gameweekno/:page', {
            //    templateUrl: 'views/globalhistorybygameweekwithgameweek.html',
            //    controller: 'GlobalhistorybygameweekwithgameweekCtrl'
            //})
            //.when('/history/global/month', {
            //    templateUrl: 'views/globalhistorybymonth.html',
            //    controller: 'GlobalhistorybymonthCtrl'
            //})
            //.when('/history/global/month/:month/:page', {
            //    templateUrl: 'views/globalhistorybymonthwithmonth.html',
            //    controller: 'GlobalhistorybymonthwithmonthCtrl'
            //})

            .when('/history/:leagueId/month', {
                templateUrl: 'views/historybymonth.html',
                controller: 'HistorybymonthCtrl'
            })
            .when('/history/:leagueId/month/:month/:page', {
                templateUrl: 'views/historybymonthwithmonth.html',
                controller: 'HistorybymonthwithmonthCtrl'
            })
            .when('/history/:leagueId/gameweek', {
                templateUrl: 'views/historybygameweek.html',
                controller: 'HistorybygameweekCtrl'
            })
            .when('/history/:leagueId/gameweek/:gameweekno/:page', {
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
            .when('/leagues', {
                templateUrl: 'views/leagues.html',
                controller: 'LeaguesCtrl'
            })
            .when('/createleague', {
                templateUrl: 'views/createleague.html',
                controller: 'CreateleagueCtrl'
            })
            .when('/league/:leagueId/:page', {
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
            .when('/matrix/:leagueId/:gameweekno/:page', {
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
            .when('/leaguedelete/:leagueId', {
                templateUrl: 'views/leaguedelete.html',
                controller: 'LeaguedeleteCtrl'
            })
            .otherwise({
                redirectTo: '/'
            });
    })
    .config(function(localStorageServiceProvider) {
        localStorageServiceProvider.setPrefix('prdlge');
    })
    .config(function(blockUIConfig) {
        blockUIConfig.message = '';
    })
    .config(function(cfpLoadingBarProvider) {
        cfpLoadingBarProvider.includeSpinner = false;
    }).run(function(googleanalytics) {
        googleanalytics.init();
    });