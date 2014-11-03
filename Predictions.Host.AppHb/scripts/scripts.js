"use strict";angular.module("frontendApp",["ngAnimate","ngCookies","ngResource","ngRoute","ngSanitize","ngTouch","LocalStorageModule","ui.bootstrap","ui.bootstrap.datetimepicker","ngToast","chart.js"]).config(["$routeProvider",function(a){a.when("/",{templateUrl:"views/main.html",controller:"MainCtrl"}).when("/leaguetable",{templateUrl:"views/leaguetable.html",controller:"LeaguetableCtrl"}).when("/player/:playerName",{templateUrl:"views/player.html",controller:"PlayerCtrl"}).when("/playergameweek/:playerName/:gameWeekNo",{templateUrl:"views/playergameweek.html",controller:"PlayergameweekCtrl"}).when("/openfixtures",{templateUrl:"views/openfixtures.html",controller:"OpenfixturesCtrl"}).when("/admin/addgameweek",{templateUrl:"views/adminaddgameweek.html",controller:"AdminaddgameweekCtrl"}).when("/admin/addresults",{templateUrl:"views/adminaddresults.html",controller:"AdminaddresultsCtrl"}).when("/pastgameweeks",{templateUrl:"views/pastgameweeks.html",controller:"PastgameweeksCtrl"}).when("/gameweekscores/:gameWeekNo",{templateUrl:"views/gameweekscores.html",controller:"GameweekscoresCtrl"}).when("/fixture/:fxid",{templateUrl:"views/fixture.html",controller:"FixtureCtrl"}).when("/editpredictions",{templateUrl:"views/editpredictions.html",controller:"EditpredictionsCtrl"}).when("/history/month",{templateUrl:"views/historybymonth.html",controller:"HistorybymonthCtrl"}).when("/history/month/:month",{templateUrl:"views/historybymonthwithmonth.html",controller:"HistorybymonthwithmonthCtrl"}).when("/history/gameweek",{templateUrl:"views/historybygameweek.html",controller:"HistorybygameweekCtrl"}).when("/history/gameweek/:gameweekno",{templateUrl:"views/historybygameweekwithgameweek.html",controller:"HistorybygameweekwithgameweekCtrl"}).otherwise({redirectTo:"/"})}]).run(function(){Chart.defaults.global.responsive=!0}),angular.module("frontendApp").controller("MainCtrl",["$scope","$http",function(){}]),angular.module("frontendApp").controller("LeaguetableCtrl",["$scope","$http",function(a,b){b.get("/api/leaguetable").success(function(b){a.model=b})}]),angular.module("frontendApp").controller("PlayerCtrl",["$scope","$http","$routeParams",function(a,b,c){var d="/api/player/"+c.playerName;b.get(d).success(function(b){a.model=b});var e="/api/leaguepositiongraphforplayer/"+c.playerName;b.get(e).success(function(b){a.labels=b.labels,a.data=b.data,a.options={animationSteps:10,scaleOverride:!0,scaleSteps:16,scaleStepWidth:-1,scaleStartValue:17}})}]),angular.module("frontendApp").controller("PlayergameweekCtrl",["$scope","$http","$routeParams",function(a,b,c){var d="/api/playergameweek/"+c.playerName+"/"+c.gameWeekNo;b.get(d).success(function(b){a.model=b})}]),angular.module("frontendApp").controller("NavCtrl",["$scope","$location","auth",function(a,b,c){a.navCollapsed=!0,c.withPlayer(function(b){a.player=b}),a.navTo=function(c){a.navCollapsed=!0;var d=c.target.href.indexOf("#")+1,e=c.target.href.substring(d);b.path(e),c.preventDefault()}}]),angular.module("frontendApp").controller("OpenfixturesCtrl",["$scope","$http",function(a,b){b.get("/api/openfixtures").success(function(b){a.model=b}),a.submitResult=function(a){var c={fixtureId:a.fxId,score:a.score};b.post("/api/prediction",c).success(function(){a.submitted=!0})}}]),angular.module("frontendApp").controller("AdminaddgameweekCtrl",["$scope","$http","$location","$modal","localStorageService","notify",function(a,b,c,d,e,f){a.teams=["Arsenal","Aston Villa","Burnley","Chelsea","Crystal Palace","Everton","Hull","Leicester","Liverpool","Man City","Man Utd","Manchester United","Newcastle","QPR","Southampton","Stoke","Sunderland","Swansea","Tottenham","West Brom","West Ham"],a.gameweek={fixtures:[{home:"",away:"",kickoff:""},{home:"",away:"",kickoff:""},{home:"",away:"",kickoff:""}]},b.get("/api/admin/getnewgameweekno").success(function(b){a.newGameweekNumber=b}),a.addFixture=function(){var b=a.gameweek.fixtures[a.gameweek.fixtures.length-1],c=angular.copy(b.kickoff);a.gameweek.fixtures.push({home:"",away:"",kickoff:c})},a.removeFixture=function(b){a.gameweek.fixtures.splice(b,1)},a.submit=function(){b.post("/api/admin/gameweek",a.gameweek).success(function(){f.success("gameweek added"),c.path("openfixtures")})},a.open=function(b,c){var e=d.open({templateUrl:"addfixturemodal.html",controller:"AddfixturemodalCtrl",resolve:{index:function(){return b},fixture:function(){return c}}});e.result.then(function(b){a.gameweek.fixtures[b.index].kickoff=b.kickoff},function(){})}}]),angular.module("frontendApp").controller("AddfixturemodalCtrl",["$scope","$modalInstance","index","fixture",function(a,b,c,d){a.fixture=d;var e=c;a.onTimeSet=function(a,c){b.close({index:e,kickoff:c})}}]),angular.module("frontendApp").controller("AdminaddresultsCtrl",["$scope","$http",function(a,b){b.get("/api/admin/getfixturesawaitingresults").success(function(b){a.model=b}),a.submitResult=function(a){var c={fixtureId:a.fxId,score:a.score};b.post("/api/admin/result",c).success(function(){a.submitted=!0})}}]),angular.module("frontendApp").service("auth",["$http",function(a){var b=function(b){a.get("/api/whoami").success(function(a){b(a)})};return{withPlayer:b}}]),angular.module("frontendApp").factory("interceptor",["$q","notify",function(a,b){return{request:function(a){return a},responseError:function(c){var d=c.statusText+" - "+c.data;return b.fail(d),a.reject(c)}}}]),angular.module("frontendApp").config(["$httpProvider",function(a){a.interceptors.push("interceptor")}]),angular.module("frontendApp").service("notify",["ngToast",function(a){var b=function(b){a.create({"class":"success",content:b})},c=function(b){a.create({"class":"danger",content:b})};return{success:b,fail:c}}]),angular.module("frontendApp").directive("submitscore",function(){return{templateUrl:"views/directives/submitscore.html",restrict:"E"}}),angular.module("frontendApp").directive("loading",function(){return{templateUrl:"views/directives/loading.html",restrict:"E",scope:{noRowsMsg:"@",model:"="}}}),angular.module("frontendApp").controller("PastgameweeksCtrl",["$scope","$http",function(a,b){b.get("/api/history/gameweek").success(function(b){a.model=b})}]),angular.module("frontendApp").controller("GameweekscoresCtrl",["$scope","$http","$routeParams",function(a,b,c){var d="/api/history/gameweek/"+c.gameWeekNo;b.get(d).success(function(b){a.model=b})}]),angular.module("frontendApp").controller("FixtureCtrl",["$scope","$http","$routeParams",function(a,b,c){var d="/api/fixture/"+c.fxid;b.get(d).success(function(b){a.model=b});var e="/api/fixturepredictiongraph/"+c.fxid;b.get(e).success(function(b){a.labels=b.labels,a.data=b.data,a.options={animationSteps:10,animateRotate:!1,animateScale:!0}})}]),angular.module("frontendApp").controller("EditpredictionsCtrl",["$scope","$http",function(a,b){b.get("/api/editpredictions").success(function(b){a.model=b}),a.submitResult=function(a){var c={predictionId:a.predictionId,score:a.score};b.post("/api/editprediction",c).success(function(){a.submitted=!0})}}]),angular.module("frontendApp").controller("HistorybymonthCtrl",["$scope","$http","$routeParams",function(a,b){var c="/api/history/month";b.get(c).success(function(b){a.model=b})}]),angular.module("frontendApp").controller("HistorybymonthwithmonthCtrl",["$scope","$http","$routeParams",function(a,b,c){var d="/api/history/month/"+c.month;b.get(d).success(function(b){a.model=b})}]),angular.module("frontendApp").controller("HistorybygameweekCtrl",["$scope","$http",function(a,b){var c="/api/history/gameweek";b.get(c).success(function(b){a.model=b})}]),angular.module("frontendApp").controller("HistorybygameweekwithgameweekCtrl",["$scope","$http","$routeParams",function(a,b,c){var d="/api/history/gameweek/"+c.gameweekno;b.get(d).success(function(b){a.model=b})}]);