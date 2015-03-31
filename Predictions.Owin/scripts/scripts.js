"use strict";angular.module("frontendApp",["ngAnimate","ngCookies","ngResource","ngRoute","ngSanitize","ngTouch","LocalStorageModule","ui.bootstrap","ui.bootstrap.datetimepicker","ngToast","chart.js","ordinal","angular-loading-bar"]).config(["$routeProvider",function(a){a.when("/",{templateUrl:"views/main.html",controller:"MainCtrl"}).when("/leaguetable",{templateUrl:"views/leaguetable.html",controller:"LeaguetableCtrl"}).when("/player/:playerId",{templateUrl:"views/player.html",controller:"PlayerCtrl"}).when("/playergameweek/:playerId/:gameWeekNo",{templateUrl:"views/playergameweek.html",controller:"PlayergameweekCtrl"}).when("/openfixtures",{templateUrl:"views/openfixtures.html",controller:"OpenfixturesCtrl"}).when("/admin/addgameweek",{templateUrl:"views/adminaddgameweek.html",controller:"AdminaddgameweekCtrl"}).when("/admin/addresults",{templateUrl:"views/adminaddresults.html",controller:"AdminaddresultsCtrl"}).when("/fixture/:fxid",{templateUrl:"views/fixture.html",controller:"FixtureCtrl"}).when("/history/month",{templateUrl:"views/historybymonth.html",controller:"HistorybymonthCtrl"}).when("/history/month/:month",{templateUrl:"views/historybymonthwithmonth.html",controller:"HistorybymonthwithmonthCtrl"}).when("/history/gameweek",{templateUrl:"views/historybygameweek.html",controller:"HistorybygameweekCtrl"}).when("/history/gameweek/:gameweekno",{templateUrl:"views/historybygameweekwithgameweek.html",controller:"HistorybygameweekwithgameweekCtrl"}).when("/admin/addresults/:gameweekno",{templateUrl:"views/adminaddresultsforgameweek.html",controller:"AdminaddresultsforgameweekCtrl"}).when("/inplay",{templateUrl:"views/inplay.html",controller:"InplayCtrl"}).when("/login",{templateUrl:"views/login.html",controller:"LoginCtrl"}).otherwise({redirectTo:"/"})}]).run(function(){Chart.defaults.global.responsive=!0}).config(["localStorageServiceProvider",function(a){a.setPrefix("prdlge")}]),angular.module("frontendApp").controller("MainCtrl",["$scope","$http","auth",function(a,b,c){c.withPlayer(function(b){a.loggedInPlayer=b}),b.get("/api/getleaguepositionforplayer").success(function(b){a.leaguePosition=b}),b.get("/api/getlastgameweekandwinner").success(function(b){a.lastgw=b}),b.get("/api/getopenfixtureswithnopredictionsforplayercount").success(function(b){a.openfixturecount=b,a.openfixturecountLoaded=!0})}]),angular.module("frontendApp").controller("LeaguetableCtrl",["$scope","$http",function(a,b){b.get("/api/leaguetable").success(function(b){a.model=b})}]),angular.module("frontendApp").controller("PlayerCtrl",["$scope","$http","$routeParams",function(a,b,c){var d="/api/player/"+c.playerId;b.get(d).success(function(b){a.model=b});var e="/api/leaguepositiongraphforplayer/"+c.playerId;b.get(e).success(function(b){a.labels=b.labels,a.data=b.data,a.options={animationSteps:10,showScale:!1,scaleOverride:!0,scaleSteps:16,scaleStepWidth:-1,scaleStartValue:17}})}]),angular.module("frontendApp").controller("PlayergameweekCtrl",["$scope","$http","$routeParams",function(a,b,c){var d="/api/playergameweek/"+c.playerId+"/"+c.gameWeekNo;b.get(d).success(function(b){a.model=b})}]),angular.module("frontendApp").controller("NavCtrl",["$scope","$location","auth",function(a,b,c){a.navCollapsed=!0,c.withPlayer(function(b){a.player=b}),a.navTo=function(c){a.navCollapsed=!0;var d=c.target.href.indexOf("#")+1,e=c.target.href.substring(d);b.path(e),c.preventDefault()}}]),angular.module("frontendApp").controller("OpenfixturesCtrl",["$scope","$http",function(a,b){function c(b){var d=a.model.rows[b];d.submitWithCallBack(d,function(){b+1<a.model.rows.length?c(b+1):a.submittingAll=!1})}b.get("/api/openfixtures").success(function(b){a.model=b}),a.anyEditableRows=function(){if(a.model)for(var b=0;b<a.model.rows.length;b++){var c=a.model.rows[b];if(c.isSubmittable(c))return!0}return!1},a.submitAll=function(){a.submittingAll=!0,c(0)}}]),angular.module("frontendApp").controller("AdminaddgameweekCtrl",["$scope","$http","$location","$modal","localStorageService","notify",function(a,b,c,d,e,f){a.teams=["Arsenal","Aston Villa","Burnley","Chelsea","Crystal Palace","Everton","Hull","Leicester","Liverpool","Man City","Man Utd","Newcastle","QPR","Southampton","Stoke","Sunderland","Swansea","Tottenham","West Brom","West Ham"],a.gameweek={fixtures:[{home:"",away:"",kickoff:""},{home:"",away:"",kickoff:""},{home:"",away:"",kickoff:""}]},a.addFixture=function(){var b=a.gameweek.fixtures[a.gameweek.fixtures.length-1],c=angular.copy(b.kickoff);a.gameweek.fixtures.push({home:"",away:"",kickoff:c})},a.removeFixture=function(b){a.gameweek.fixtures.splice(b,1)},a.submit=function(){a.inSubmission=!0,b.post("/api/admin/gameweek",a.gameweek).success(function(){a.inSubmission=!1,f.success("gameweek added"),c.path("openfixtures")}).error(function(){a.inSubmission=!1})},a.open=function(b,c){var e=d.open({templateUrl:"addfixturemodal.html",controller:"AddfixturemodalCtrl",size:"sm",resolve:{index:function(){return b},fixture:function(){return c}}});e.result.then(function(b){a.gameweek.fixtures[b.index].kickoff=b.kickoff},function(){})}}]),angular.module("frontendApp").controller("AddfixturemodalCtrl",["$scope","$modalInstance","index","fixture",function(a,b,c,d){a.fixture=d;var e=c;a.onTimeSet=function(a,c){b.close({index:e,kickoff:c})}}]),angular.module("frontendApp").controller("AdminaddresultsCtrl",["$scope","$http",function(a,b){b.get("/api/admin/getgameweekswithclosedfixtures").success(function(b){a.model=b})}]),angular.module("frontendApp").service("auth",["$http",function(a){var b=function(b){a.get("/api/whoami").success(function(a){b(a)})};return{withPlayer:b}}]),angular.module("frontendApp").factory("interceptor",["$q","$location","notify",function(a,b,c){return{responseError:function(d){if(401===d.status)b.path("/login");else{var e=d.statusText+" - "+d.data;c.fail(e)}return a.reject(d)}}}]),angular.module("frontendApp").config(["$httpProvider",function(a){a.interceptors.push("interceptor")}]),angular.module("frontendApp").service("notify",["ngToast",function(a){var b=function(b){var c="<span class='glyphicon glyphicon-ok'></span> "+b;a.create({"class":"success",content:c})},c=function(b){var c="<span class='glyphicon glyphicon-ban-circle'></span> "+b;a.create({"class":"danger",content:c})};return{success:b,fail:c}}]),angular.module("frontendApp").directive("focusOn",function(){return function(a,b,c){a.$on(c.focusOn,function(){b[0].focus()})}}),angular.module("frontendApp").directive("submitscore",["$http","$timeout","notify",function(a,b,c){return{templateUrl:"views/directives/submitscore.html",restrict:"E",scope:{row:"=",postUrl:"@"},link:function(d){function e(a,b){return["Successfully submitted ",a.fixture.home," ",b.home," - ",b.away," ",a.fixture.away].join("")}function f(){d.row.state=d.row.scoreSubmitted?g.readonly:g.create}var g={readonly:"readonly",create:"create",edit:"edit"};d.focused="",d.focus=function(){d.focused="focused"},d.blur=function(){d.focused=""},f();var h=null,i=null;d.setCreateScoreForm=function(a){h=a},d.setEditScoreForm=function(a){i=a},d.row.isSubmittable=function(a){switch(a.state){case g.create:return h&&h.$valid;case g.edit:return i&&i.$valid;case g.readonly:default:return!1}},d.enterEditMode=function(a){a.state=g.edit,a.existingScoreOriginal=angular.copy(a.existingScore),b(function(){d.$broadcast("editModeEntered")})},d.enterReadOnlyMode=function(a){a.state=g.readonly,a.existingScore=a.existingScoreOriginal},d.row.submit=function(a){d.row.submitWithCallBack(a,function(){})},d.row.submitWithCallBack=function(a,b){switch(a.state){case g.create:h.$valid?j(a,b):b();break;case g.edit:i.$valid?k(a,b):b();break;case g.readonly:default:b()}};var j=function(b,f){var g=e(b,b.newScore),h={fixtureId:b.fixture.fxId,score:b.newScore};d.inSubmission=!0,a.post(d.postUrl,h).success(function(){c.success(g),d.enterReadOnlyMode(b),b.existingScore=b.newScore,d.inSubmission=!1,f()}).error(function(){d.inSubmission=!1,f()})},k=function(b,f){var g=e(b,b.existingScore),h={fixtureId:b.fixture.fxId,score:b.existingScore};d.inSubmission=!0,a.post(d.postUrl,h).success(function(){c.success(g),b.existingScoreOriginal=b.existingScore,d.enterReadOnlyMode(b),d.inSubmission=!1,f()}).error(function(){d.inSubmission=!1,f()})}}}}]),angular.module("frontendApp").directive("loading",function(){return{templateUrl:"views/directives/loading.html",restrict:"E",scope:{noRowsMsg:"@",model:"="}}}),angular.module("frontendApp").controller("FixtureCtrl",["$scope","$http","$routeParams",function(a,b,c){var d="/api/fixture/"+c.fxid;b.get(d).success(function(b){a.model=b});var e="/api/fixturepredictiongraph/"+c.fxid;b.get(e).success(function(b){a.labels=b.labels,a.data=b.data,a.options={animationSteps:10,animateRotate:!1,animateScale:!0}})}]),angular.module("frontendApp").controller("HistorybymonthCtrl",["$scope","$http","$routeParams",function(a,b){var c="/api/history/month";b.get(c).success(function(b){a.model=b})}]),angular.module("frontendApp").controller("HistorybymonthwithmonthCtrl",["$scope","$http","$routeParams",function(a,b,c){var d="/api/history/month/"+c.month;b.get(d).success(function(b){a.model=b})}]),angular.module("frontendApp").controller("HistorybygameweekCtrl",["$scope","$http",function(a,b){var c="/api/history/gameweek";b.get(c).success(function(b){a.model=b})}]),angular.module("frontendApp").controller("HistorybygameweekwithgameweekCtrl",["$scope","$http","$routeParams",function(a,b,c){var d="/api/history/gameweek/"+c.gameweekno;b.get(d).success(function(b){a.model=b})}]),angular.module("frontendApp").controller("AdminaddresultsforgameweekCtrl",["$scope","$http","$routeParams",function(a,b,c){var d="/api/admin/getclosedfixturesforgameweek/"+c.gameweekno;b.get(d).success(function(b){a.model=b})}]),angular.module("frontendApp").controller("InplayCtrl",["$scope","$http",function(a,b){b.get("/api/inplay").success(function(b){a.model=b})}]),angular.module("frontendApp").directive("formguide",function(){return{template:"<ul class='form-guide'><li ng-repeat='g in guide track by $index' class='outcome-{{g}}'><span>{{g}}</span></li></ul>",restrict:"E",scope:{guide:"="}}}),angular.module("frontendApp").controller("LoginCtrl",["$scope","$location","oauthService",function(){}]);