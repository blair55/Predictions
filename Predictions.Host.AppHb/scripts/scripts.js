"use strict";angular.module("frontendApp",["ngAnimate","ngCookies","ngResource","ngRoute","ngSanitize","ngTouch","LocalStorageModule","ui.bootstrap","ngToast","chart.js","ordinal","angular-loading-bar","blockUI"]).config(["$routeProvider",function(a){a.when("/",{templateUrl:"views/main.html",controller:"MainCtrl"}).when("/player/:playerId/:leagueId",{templateUrl:"views/player.html",controller:"PlayerCtrl"}).when("/playergameweek/:playerId/:gameWeekNo",{templateUrl:"views/playergameweek.html",controller:"PlayergameweekCtrl"}).when("/playerprofile/:playerId",{templateUrl:"views/playerprofile.html",controller:"PlayerprofileCtrl"}).when("/openfixtures",{templateUrl:"views/openfixtures.html",controller:"OpenfixturesCtrl"}).when("/admin/addgameweek",{templateUrl:"views/adminaddgameweek.html",controller:"AdminaddgameweekCtrl"}).when("/admin/addresults",{templateUrl:"views/adminaddresults.html",controller:"AdminaddresultsCtrl"}).when("/fixture/:fxid",{templateUrl:"views/fixture.html",controller:"FixtureCtrl"}).when("/history/:leagueId/month",{templateUrl:"views/historybymonth.html",controller:"HistorybymonthCtrl"}).when("/history/:leagueId/month/:month/:page",{templateUrl:"views/historybymonthwithmonth.html",controller:"HistorybymonthwithmonthCtrl"}).when("/history/:leagueId/gameweek",{templateUrl:"views/historybygameweek.html",controller:"HistorybygameweekCtrl"}).when("/history/:leagueId/gameweek/:gameweekno/:page",{templateUrl:"views/historybygameweekwithgameweek.html",controller:"HistorybygameweekwithgameweekCtrl"}).when("/admin/addresults/:gameweekno",{templateUrl:"views/adminaddresultsforgameweek.html",controller:"AdminaddresultsforgameweekCtrl"}).when("/inplay",{templateUrl:"views/inplay.html",controller:"InplayCtrl"}).when("/leagues",{templateUrl:"views/leagues.html",controller:"LeaguesCtrl"}).when("/createleague",{templateUrl:"views/createleague.html",controller:"CreateleagueCtrl"}).when("/league/:leagueId/:page",{templateUrl:"views/league.html",controller:"LeagueCtrl"}).when("/leagueinvite/:leagueId",{templateUrl:"views/leagueinvite.html",controller:"LeagueinviteCtrl"}).when("/joinleague/:shareableLeagueId",{templateUrl:"views/leaguejoin.html",controller:"LeaguejoinCtrl"}).when("/matrix/:leagueId/:gameweekno/:page",{templateUrl:"views/matrix.html",controller:"MatrixCtrl"}).when("/globalleague/:page",{templateUrl:"views/globalleague.html",controller:"GloballeagueCtrl"}).when("/leagueleave/:leagueId",{templateUrl:"views/leagueleave.html",controller:"LeagueleaveCtrl"}).when("/leaguedelete/:leagueId",{templateUrl:"views/leaguedelete.html",controller:"LeaguedeleteCtrl"}).otherwise({redirectTo:"/"})}]).config(["localStorageServiceProvider",function(a){a.setPrefix("prdlge")}]).config(["blockUIConfig",function(a){a.message=""}]).config(["cfpLoadingBarProvider",function(a){a.includeSpinner=!1}]).run(["googleanalytics",function(a){a.init()}]),angular.module("frontendApp").controller("MainCtrl",["$scope","$http","auth","title",function(a,b,c,d){d.set("Predictions League"),c.withPlayer(function(b){a.loggedInPlayer=b}),b.get("/api/getleaguepositionforplayer").success(function(b){a.global=b}),b.get("/api/getlastgameweekandwinner").success(function(b){a.lastgw=b}),b.get("/api/getopenfixtureswithnopredictionsforplayercount").success(function(b){a.openfixturecount=b}),b.get("/api/leagues").success(function(b){a.leagues=b})}]),angular.module("frontendApp").controller("PlayerCtrl",["$scope","$http","$routeParams","title",function(a,b,c,d){var e="/api/player/"+c.playerId+"/"+c.leagueId;b.get(e).success(function(b){a.model=b,d.set(b.league.name+" / "+b.player.name)});var f="/api/leaguepositiongraphforplayer/"+c.playerId+"/"+c.leagueId;b.get(f).success(function(b){a.labels=b.labels,a.data=b.data,a.options={animationSteps:10,showScale:!1,scaleOverride:!0,scaleSteps:b.scaleSteps-1,scaleStepWidth:-1,scaleStartValue:b.scaleSteps}})}]),angular.module("frontendApp").controller("PlayergameweekCtrl",["$scope","$http","$routeParams","title",function(a,b,c,d){var e="/api/playergameweek/"+c.playerId+"/"+c.gameWeekNo;b.get(e).success(function(b){a.model=b,d.set(b.player.name+" / GW#"+b.gameWeekNo)});var f="/api/gameweekneighbours/"+c.gameWeekNo;b.get(f).success(function(b){a.neighbours=b})}]),angular.module("frontendApp").controller("NavCtrl",["$scope","$location","auth",function(a,b,c){a.navCollapsed=!0,c.withPlayer(function(b){a.player=b}),a.navTo=function(c){a.navCollapsed=!0;var d=c.target.href.indexOf("#")+1,e=c.target.href.substring(d);b.path(e),c.preventDefault()}}]),angular.module("frontendApp").controller("OpenfixturesCtrl",["$scope","$http","title",function(a,b,c){function d(b){var c=a.model.rows[b];c.submitWithCallBack(c,function(){b+1<a.model.rows.length?d(b+1):a.submittingAll=!1})}c.set("Open Fixtures"),b.get("/api/openfixtures").success(function(b){a.model=b}),a.anyEditableRows=function(){if(a.model)for(var b=0;b<a.model.rows.length;b++){var c=a.model.rows[b];if(c.isSubmittable(c))return!0}return!1},a.$on("doubleDownSet",function(b,c){for(var d=0;d<a.model.rows.length;d++){var e=a.model.rows[d];e.fixture.gameWeekNumber==c.gwno&&e.clearDoubleDown(e)}}),a.submitAll=function(){a.submittingAll=!0,d(0)}}]),angular.module("frontendApp").controller("AdminaddgameweekCtrl",["$scope","$http","$location","notify","title",function(a,b,c,d,e){var f="/api/admin/gameweek";b.get(f).success(function(b){a.model=b,e.set("Add Game Week")}),a.submit=function(){b.post(f).success(function(){d.success("gameweek added successfully"),c.path("openfixtures")})}}]),angular.module("frontendApp").controller("AdminaddresultsCtrl",["$scope","$http","title",function(a,b,c){b.get("/api/admin/getgameweekswithclosedfixtures").success(function(b){a.model=b,c.set("Add Results")})}]),angular.module("frontendApp").service("auth",["$http",function(a){var b=function(b){a.get("/api/whoami").success(function(a){b(a)})};return{withPlayer:b}}]),angular.module("frontendApp").factory("interceptor",["$q","$location","$window","notify",function(a,b,c,d){return{responseError:function(b){if(401===b.status){var e=c.location.hash.substr(1),f="/login.html?redirect="+e;c.location.href=f}else{var g=b.statusText+" - "+b.data;d.fail(g)}return a.reject(b)}}}]),angular.module("frontendApp").config(["$httpProvider",function(a){a.interceptors.push("interceptor")}]),angular.module("frontendApp").service("notify",["ngToast",function(a){var b=function(b){var c="<span class='glyphicon glyphicon-ok'></span> "+b;a.create({"class":"success",content:c})},c=function(b){var c="<span class='glyphicon glyphicon-ban-circle'></span> "+b;a.create({"class":"danger",content:c})};return{success:b,fail:c}}]),angular.module("frontendApp").directive("submitscore",["$http","$timeout","$location","notify",function(a,b,c,d){return{templateUrl:"views/directives/submitscore.html",restrict:"E",scope:{row:"=",postUrl:"@",minMode:"="},link:function(e,f,g){function h(a,b){return[a.fixture.home.full," ",b.home," - ",b.away," ",a.fixture.away.full].join("")}function i(){e.row.state=e.row.scoreSubmitted?j.readonly:j.create}var j={readonly:"readonly",create:"create",edit:"edit"};e.focused="",e.focus=function(){e.focused="focused"},e.blur=function(){e.focused=""},i();var k=null,l=null;e.setCreateScoreForm=function(a){k=a},e.setEditScoreForm=function(a){l=a},e.enterEditMode=function(a){a.state=j.edit,a.existingScoreOriginal=angular.copy(a.existingScore),b(function(){e.$broadcast("editModeEntered")})},e.enterReadOnlyMode=function(a){a.state=j.readonly,a.existingScore=a.existingScoreOriginal},e.navToFixture=function(a){c.path("/fixture/"+a.fixture.fxId)},e.row.isSubmittable=function(a){switch(a.state){case j.create:return k&&k.$valid;case j.edit:return l&&l.$valid;case j.readonly:default:return!1}},e.row.submit=function(a){e.row.submitWithCallBack(a,function(){})},e.row.submitWithCallBack=function(a,b){switch(a.state){case j.create:k.$valid?m(a,b):b();break;case j.edit:l.$valid?n(a,b):b();break;case j.readonly:default:b()}};var m=function(b,c){var f=h(b,b.newScore),g={fixtureId:b.fixture.fxId,score:b.newScore};e.inSubmission=!0,a.post(e.postUrl,g).success(function(a){d.success(f),e.enterReadOnlyMode(b),b.existingScore=b.newScore,b.predictionId=a.predictionId,e.inSubmission=!1,e.$broadcast("predictionSubmitted",{row:b}),c()}).error(function(a,b,d,f){e.inSubmission=!1,c()})},n=function(b,c){var f=h(b,b.existingScore),g={fixtureId:b.fixture.fxId,score:b.existingScore};e.inSubmission=!0,a.post(e.postUrl,g).success(function(a){d.success(f),b.existingScoreOriginal=b.existingScore,e.enterReadOnlyMode(b),e.inSubmission=!1,c()}).error(function(a,b,d,f){e.inSubmission=!1,c()})}}}}]),angular.module("frontendApp").controller("FixtureCtrl",["$scope","$http","$routeParams","title",function(a,b,c,d){var e="/api/fixture/"+c.fxid;b.get(e).success(function(b){a.model=b,d.set(b.fixture.home.full.toUpperCase()+" v "+b.fixture.away.full.toUpperCase())});var f="/api/fixturepredictiongraph/"+c.fxid;b.get(f).success(function(b){a.labels=b.labels,a.data=b.data,a.options={animationSteps:10,animateRotate:!1,animateScale:!0}});var g="/api/fixtureDoubleDowns/"+c.fxid;b.get(g).success(function(b){a.doubleDowns=b});var h="/api/fixtureformguide/"+c.fxid;b.get(h).success(function(b){a.formGuide=b});var i="/api/fixturepreviousmeetings/"+c.fxid;b.get(i).success(function(b){a.previousMeetings=b,a.rebindFixtureHistory()}),a.rebindFixtureHistory=function(){a.previousMeetings.rows=a.previousMeetings.showAll?a.previousMeetings.allRows:a.previousMeetings.thisFixtureRows};var j="/api/getfixtureneighbours/"+c.fxid;b.get(j).success(function(b){a.neighbours=b})}]),angular.module("frontendApp").controller("HistorybymonthCtrl",["$scope","$http","$routeParams","title",function(a,b,c,d){var e="/api/leaguehistory/"+c.leagueId+"/month";b.get(e).success(function(b){a.model=b,d.set(b.league.name+" / Month History")})}]),angular.module("frontendApp").controller("HistorybymonthwithmonthCtrl",["$scope","$http","$routeParams","auth","title",function(a,b,c,d,e){var f="/api/leaguehistory/"+c.leagueId+"/month/"+c.month+"/page/"+c.page;b.get(f).success(function(b){a.model=b,e.set(b.league.name+" / "+b.month)});var g="/api/monthneighbours/"+c.month;b.get(g).success(function(b){a.neighbours=b}),d.withPlayer(function(b){a.loggedInPlayer=b})}]),angular.module("frontendApp").controller("HistorybygameweekCtrl",["$scope","$http","$routeParams","title",function(a,b,c,d){var e="/api/leaguehistory/"+c.leagueId+"/gameweek";b.get(e).success(function(b){a.model=b,d.set(b.league.name+" / GW History")})}]),angular.module("frontendApp").controller("HistorybygameweekwithgameweekCtrl",["$scope","$http","$routeParams","auth","title",function(a,b,c,d,e){var f="/api/leaguehistory/"+c.leagueId+"/gameweek/"+c.gameweekno+"/page/"+c.page;b.get(f).success(function(b){a.model=b,e.set(b.league.name+" / GW#"+b.gameWeekNo+" Table")});var g="/api/gameweekneighbours/"+c.gameweekno;b.get(g).success(function(b){a.neighbours=b}),d.withPlayer(function(b){a.loggedInPlayer=b})}]),angular.module("frontendApp").controller("AdminaddresultsforgameweekCtrl",["$scope","$http","$routeParams","title",function(a,b,c,d){var e="/api/admin/getclosedfixturesforgameweek/"+c.gameweekno;b.get(e).success(function(b){a.model=b,d.set("GW#"+b.gameWeekNo+" / Add Results")})}]),angular.module("frontendApp").controller("InplayCtrl",["$scope","$http","title",function(a,b,c){b.get("/api/inplay").success(function(b){a.model=b,c.set("Active Game Week")})}]),angular.module("frontendApp").directive("formguide",function(){return{template:"<ul class='form-guide'><li ng-repeat='g in guide track by $index' class='outcome-{{g}}'><span>{{g}}</span></li></ul>",restrict:"E",scope:{guide:"="}}}),angular.module("frontendApp").controller("LeaguesCtrl",["$scope","$http","title",function(a,b,c){b.get("/api/leagues").success(function(b){a.model=b,c.set("My Leagues")})}]),angular.module("frontendApp").controller("LeagueCtrl",["$scope","$http","$routeParams","auth","title",function(a,b,c,d,e){var f="/api/league/"+c.leagueId+"/"+c.page;b.get(f).success(function(b){a.model=b,e.set(b.name),d.withPlayer(function(c){a.loggedInPlayer=c,a.isLoggedInPlayerLeagueAdmin=c.id==b.adminId})})}]),angular.module("frontendApp").controller("CreateleagueCtrl",["$scope","$http","$location","notify","title",function(a,b,c,d,e){e.set("Create League"),a.submit=function(){var e={name:a.leagueName};b.post("/api/createleague",e).success(function(a){d.success(a.name+" created successfully"),c.path("leagueinvite/"+a.id)})}}]),angular.module("frontendApp").controller("LeagueinviteCtrl",["$scope","$http","$routeParams","title",function(a,b,c,d){var e="/api/leagueinvite/"+c.leagueId;b.get(e).success(function(b){a.model=b,d.set("Invite to "+b.name),a.model.encodedInviteLink=encodeURIComponent(b.inviteLink),a.model.encodedShareText=encodeURIComponent("Join my Predictions League!")}),a.share=function(){FB.ui({method:"share",caption:"Join my Predictions League!",href:a.model.inviteLink})}}]),angular.module("frontendApp").controller("LeaguejoinCtrl",["$scope","$http","$routeParams","$location","notify","title",function(a,b,c,d,e,f){f.set("Join League");var g="/api/leaguejoin/"+c.shareableLeagueId;b.get(g).success(function(b){a.model=b}),a.submit=function(){var c="/api/leaguejoin/"+a.model.id;b.post(c).success(function(a){e.success(a.name+" joined successfully"),d.path("league/"+a.id)})}}]),angular.module("frontendApp").controller("MatrixCtrl",["$scope","$http","$routeParams","auth","title",function(a,b,c,d,e){var f="/api/gameweekmatrix/"+c.leagueId+"/gameweek/"+c.gameweekno+"/page/"+c.page;b.get(f).success(function(b){a.model=b,e.set(b.league.name+" / GW#"+b.gameWeekNo+" Matrix")}),d.withPlayer(function(b){a.loggedInPlayer=b})}]),angular.module("frontendApp").controller("PlayerprofileCtrl",["$scope","$http","$routeParams","title",function(a,b,c,d){var e="/api/playerprofile/"+c.playerId;b.get(e).success(function(b){a.model=b,d.set(b.player.name)});var f="/api/playerprofilegraph/"+c.playerId;b.get(f).success(function(b){a.labels=b.labels,a.data=b.data,a.options={animationSteps:10,showScale:!1,scaleOverride:!1}})}]),angular.module("frontendApp").controller("LeagueleaveCtrl",["$scope","$http","$routeParams","$location","notify","title",function(a,b,c,d,e,f){f.set("Leave League");var g="/api/league/"+c.leagueId;b.get(g).success(function(b){a.model=b}),a.submit=function(){var c="/api/leagueleave/"+a.model.id;b.post(c).success(function(){e.success("League left successfully"),d.path("leagues")})}}]),angular.module("frontendApp").directive("previousmeetingrow",function(){return{template:'<td class="cell-15"><span class="label label-primary">{{ row.kickoff | date:"MMM yyyy" }}</span></td><td class="cell-25 text-right"><span class="text-uppercase"><teamname team="row.home"></teamname></span></td><td class="cell-20 text-center">{{row.homeTeamScore}} - {{row.awayTeamScore}}</td><td class="cell-40"><span class="text-uppercase"><teamname team="row.away"></teamname></span></td>',restrict:"EA",scope:{row:"=previousmeetingrow"}}}),angular.module("frontendApp").directive("fixtureformguide",function(){return{template:'<div ng-switch="guide" class="fixture-form-guide"><span ng-switch-when="w" class="label label-success">W</span><span ng-switch-when="l" class="label label-danger">L</span><span ng-switch-when="d" class="label label-default">D</span></div>',restrict:"E",scope:{guide:"="}}}),angular.module("frontendApp").directive("facebooklike",["$window",function(a){return{restrict:"A",scope:{facebooklike:"=?"},link:function(b,c,d){function e(){if(!d.fbLike||b.fbLike||f)setTimeout(function(){c.html('<div class="fb-like"'+(b.fbLike?' data-href="'+b.fbLike+'"':"")+' data-layout="button_count" data-action="like" data-show-faces="true" data-share="true"></div>'),a.FB.XFBML.parse(c.parent()[0])},100);else{f=!0;var g=b.$watch("facebooklike",function(a,b){a&&(e(),g())})}}e();var f=!1}}}]),angular.module("frontendApp").directive("twittertweet",["$window","$location",function(a,b){return{restrict:"A",scope:{twittertweet:"=",tweetUrl:"="},link:function(c,d,e){function f(){if(c.twittertweet||g)setTimeout(function(){d.html('<a href="https://twitter.com/share" class="twitter-share-button" data-text="'+c.twittertweet+'" data-url="'+(c.tweetUrl||b.absUrl())+'">Tweet</a>'),a.twttr.widgets.load(d.parent()[0])},100);else{g=!0;var e=c.$watch("twittertweet",function(a,b){a&&(f(),e())})}}f();var g=!1}}}]),angular.module("frontendApp").controller("LeaguedeleteCtrl",["$scope","$http","$routeParams","$location","notify","title",function(a,b,c,d,e,f){f.set("Delete League");var g="/api/league/"+c.leagueId;b.get(g).success(function(b){a.model=b}),a.submit=function(){var c="/api/leaguedelete/"+a.model.id;b.post(c).success(function(){e.success("League deleted successfully"),d.path("leagues")})}}]),angular.module("frontendApp").service("googleanalytics",["$rootScope","$window","$location",function(a,b,c){var d=function(){b.ga&&a.$on("$routeChangeSuccess",function(){b.ga("send","pageview",{page:c.url()})})};return{init:d}}]),angular.module("frontendApp").directive("twitterfollow",["$window",function(a){return{restrict:"A",link:function(b,c,d){setTimeout(function(){c.html('<a href="https://twitter.com/predictionslge" class="twitter-follow-button">Follow @PredictionsLge</a>'),a.twttr.widgets.load(c.parent()[0])},100)}}}]),angular.module("frontendApp").directive("doubledownbutton",["$timeout","$http","notify",function(a,b,c){return{template:'<div class="dd" ng-class="ddClass"><span class="label dd-button" ng-click="trySetDoubleDown(row)"><i class="glyphicon glyphicon-unchecked"></i><i class="glyphicon glyphicon-check"></i>&nbsp;Double Down</span></div>',restrict:"E",scope:{row:"="},link:function(d){var e=function(a){"create"==d.row.state?d.ddClass="dd-unselectable":d.ddClass=d.row.isDoubleDown?"dd-selected":"dd-selectable"};d.trySetDoubleDown=function(f){if("create"==f.state)c.fail("Submit prediction before using Double Down");else if(!f.isDoubleDown){var g="/api/doubledown/"+f.predictionId;b.post(g).success(function(){c.success("Double Down on "+f.fixture.home.full+" v "+f.fixture.away.full),d.$emit("doubleDownSet",{gwno:f.fixture.gameWeekNumber}),a(function(){f.isDoubleDown=!0,e(f)},10)})}},d.$on("predictionSubmitted",function(a,b){e(b.row)}),d.row.clearDoubleDown=function(a){a.isDoubleDown=!1,e(a)},e(d.row)}}}]),angular.module("frontendApp").directive("doubledownlocked",function(){return{template:'<div ng-if="row.isDoubleDown" class="dd dd-selected"><span class="label dd-button"><i class="glyphicon glyphicon-lock"></i>&nbsp;Double Down</span></div>',restrict:"E",scope:{row:"="}}}),angular.module("frontendApp").directive("teamname",function(){return{template:'<span class="team-name-full">{{team.full}}</span><span class="team-name-abrv">{{team.abrv}}</span>',restrict:"E",scope:{team:"="}}}),angular.module("frontendApp").directive("neighbours",function(){return{template:'<nav ng-if="neighbours"><ul class="pager"><li class="previous" ng-if="neighbours.prev"><a ng-href="{{url + neighbours.prev}}"><span aria-hidden="true">&larr;</span> Prev</a></li><li class="next" ng-if="neighbours.next"><a ng-href="{{url + neighbours.next}}">Next <span aria-hidden="true">&rarr;</span></a></li></ul></nav>',restrict:"A",scope:{url:"@",neighbours:"=?"}}}),angular.module("frontendApp").service("title",["$rootScope",function(a){return a.$on("$routeChangeSuccess",function(b,c){a.title=""}),{set:function(b){a.title=b}}}]),angular.module("frontendApp").directive("pagetitle",function(){return{template:'<h4 class="page-title" ng-if="title" ng-bind="title"></h4>',restrict:"E"}});