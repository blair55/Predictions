"use strict";angular.module("frontendApp",["ngAnimate","ngCookies","ngResource","ngRoute","ngSanitize","ngTouch","LocalStorageModule","ui.bootstrap","ui.bootstrap.datetimepicker","ngToast","chart.js","ordinal"]).config(["$routeProvider",function(a){a.when("/",{templateUrl:"views/main.html",controller:"MainCtrl"}).when("/leaguetable",{templateUrl:"views/leaguetable.html",controller:"LeaguetableCtrl"}).when("/player/:playerName",{templateUrl:"views/player.html",controller:"PlayerCtrl"}).when("/playergameweek/:playerName/:gameWeekNo",{templateUrl:"views/playergameweek.html",controller:"PlayergameweekCtrl"}).when("/openfixtures",{templateUrl:"views/openfixtures.html",controller:"OpenfixturesCtrl"}).when("/admin/addgameweek",{templateUrl:"views/adminaddgameweek.html",controller:"AdminaddgameweekCtrl"}).when("/admin/addresults",{templateUrl:"views/adminaddresults.html",controller:"AdminaddresultsCtrl"}).when("/pastgameweeks",{templateUrl:"views/pastgameweeks.html",controller:"PastgameweeksCtrl"}).when("/gameweekscores/:gameWeekNo",{templateUrl:"views/gameweekscores.html",controller:"GameweekscoresCtrl"}).when("/fixture/:fxid",{templateUrl:"views/fixture.html",controller:"FixtureCtrl"}).when("/editpredictions",{templateUrl:"views/editpredictions.html",controller:"EditpredictionsCtrl"}).when("/history/month",{templateUrl:"views/historybymonth.html",controller:"HistorybymonthCtrl"}).when("/history/month/:month",{templateUrl:"views/historybymonthwithmonth.html",controller:"HistorybymonthwithmonthCtrl"}).when("/history/gameweek",{templateUrl:"views/historybygameweek.html",controller:"HistorybygameweekCtrl"}).when("/history/gameweek/:gameweekno",{templateUrl:"views/historybygameweekwithgameweek.html",controller:"HistorybygameweekwithgameweekCtrl"}).when("/admin/addresults/:gameweekno",{templateUrl:"views/adminaddresultsforgameweek.html",controller:"AdminaddresultsforgameweekCtrl"}).otherwise({redirectTo:"/"})}]).run(function(){Chart.defaults.global.responsive=!0,window.mobilecheck=function(){var a=!1;return function(b){(/(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino/i.test(b)||/1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(b.substr(0,4)))&&(a=!0)}(navigator.userAgent||navigator.vendor||window.opera),a}}),angular.module("frontendApp").controller("MainCtrl",["$scope","$http",function(a,b){b.get("/api/getleaguepositionforplayer").success(function(b){a.leaguePosition=b}),b.get("/api/getlastgameweekandwinner").success(function(b){a.lastgw=b}),b.get("/api/getopenfixtureswithnopredictionsforplayercount").success(function(b){a.openfixturecount=b,a.openfixturecountLoaded=!0})}]),angular.module("frontendApp").controller("LeaguetableCtrl",["$scope","$http",function(a,b){b.get("/api/leaguetable").success(function(b){a.model=b})}]),angular.module("frontendApp").controller("PlayerCtrl",["$scope","$http","$routeParams",function(a,b,c){var d="/api/player/"+c.playerName;b.get(d).success(function(b){a.model=b});var e="/api/leaguepositiongraphforplayer/"+c.playerName;b.get(e).success(function(b){a.labels=b.labels,a.data=b.data,a.options={animationSteps:10,showScale:!window.mobilecheck(),scaleOverride:!0,scaleSteps:16,scaleStepWidth:-1,scaleStartValue:17}})}]),angular.module("frontendApp").controller("PlayergameweekCtrl",["$scope","$http","$routeParams",function(a,b,c){var d="/api/playergameweek/"+c.playerName+"/"+c.gameWeekNo;b.get(d).success(function(b){a.model=b})}]),angular.module("frontendApp").controller("NavCtrl",["$scope","$location","auth",function(a,b,c){a.navCollapsed=!0,c.withPlayer(function(b){a.player=b}),a.navTo=function(c){a.navCollapsed=!0;var d=c.target.href.indexOf("#")+1,e=c.target.href.substring(d);b.path(e),c.preventDefault()}}]),angular.module("frontendApp").controller("OpenfixturesCtrl",["$scope","$http",function(a,b){b.get("/api/openfixtures").success(function(b){a.model=b})}]),angular.module("frontendApp").controller("AdminaddgameweekCtrl",["$scope","$http","$location","$modal","localStorageService","notify",function(a,b,c,d,e,f){a.teams=["Arsenal","Aston Villa","Burnley","Chelsea","Crystal Palace","Everton","Hull","Leicester","Liverpool","Man City","Man Utd","Manchester United","Newcastle","QPR","Southampton","Stoke","Sunderland","Swansea","Tottenham","West Brom","West Ham"],a.gameweek={fixtures:[{home:"",away:"",kickoff:""},{home:"",away:"",kickoff:""},{home:"",away:"",kickoff:""}]},a.addFixture=function(){var b=a.gameweek.fixtures[a.gameweek.fixtures.length-1],c=angular.copy(b.kickoff);a.gameweek.fixtures.push({home:"",away:"",kickoff:c})},a.removeFixture=function(b){a.gameweek.fixtures.splice(b,1)},a.submit=function(){a.inSubmission=!0,b.post("/api/admin/gameweek",a.gameweek).success(function(){a.inSubmission=!1,f.success("gameweek added"),c.path("openfixtures")}).error(function(){a.inSubmission=!1})},a.open=function(b,c){var e=d.open({templateUrl:"addfixturemodal.html",controller:"AddfixturemodalCtrl",resolve:{index:function(){return b},fixture:function(){return c}}});e.result.then(function(b){a.gameweek.fixtures[b.index].kickoff=b.kickoff},function(){})}}]),angular.module("frontendApp").controller("AddfixturemodalCtrl",["$scope","$modalInstance","index","fixture",function(a,b,c,d){a.fixture=d;var e=c;a.onTimeSet=function(a,c){b.close({index:e,kickoff:c})}}]),angular.module("frontendApp").controller("AdminaddresultsCtrl",["$scope","$http",function(a,b){b.get("/api/admin/getgameweekswithclosedfixtures").success(function(b){a.model=b})}]),angular.module("frontendApp").service("auth",["$http",function(a){var b=function(b){a.get("/api/whoami").success(function(a){b(a)})};return{withPlayer:b}}]),angular.module("frontendApp").factory("interceptor",["$q","notify",function(a,b){return{request:function(a){return a},responseError:function(c){var d=c.statusText+" - "+c.data;return b.fail(d),a.reject(c)}}}]),angular.module("frontendApp").config(["$httpProvider",function(a){a.interceptors.push("interceptor")}]),angular.module("frontendApp").service("notify",["ngToast",function(a){var b=function(b){var c="<span class='glyphicon glyphicon-ok'></span> "+b;a.create({"class":"success",content:c})},c=function(b){var c="<span class='glyphicon glyphicon-ban-circle'></span> "+b;a.create({"class":"danger",content:c})};return{success:b,fail:c}}]),angular.module("frontendApp").directive("submitscore",["$http","notify",function(a,b){return{templateUrl:"views/directives/submitscore.html",restrict:"E",scope:{row:"=",postUrl:"@"},link:function(c){function d(a,b){return["Successfully submitted ",a.fixture.home," ",b.home," v ",b.away," ",a.fixture.away].join("")}c.enterEditMode=function(a){a.editing=!0,a.existingScoreOriginal=angular.copy(a.existingScore)},c.exitEditMode=function(a){a.editing=!1,a.existingScore=a.existingScoreOriginal},c.createScore=function(e){var f=d(e,e.newScore),g={fixtureId:e.fixture.fxId,score:e.newScore};c.inSubmission=!0,a.post(c.postUrl,g).success(function(){b.success(f),e.scoreSubmitted=!0,e.existingScore=e.newScore,c.inSubmission=!1}).error(function(){c.inSubmission=!1})},c.editScore=function(e){var f=d(e,e.existingScore),g={fixtureId:e.fixture.fxId,score:e.existingScore};c.inSubmission=!0,a.post(c.postUrl,g).success(function(){b.success(f),e.existingScoreOriginal=e.existingScore,c.exitEditMode(e),c.inSubmission=!1}).error(function(){c.inSubmission=!1})}}}}]),angular.module("frontendApp").directive("loading",function(){return{templateUrl:"views/directives/loading.html",restrict:"E",scope:{noRowsMsg:"@",model:"="}}}),angular.module("frontendApp").controller("PastgameweeksCtrl",["$scope","$http",function(a,b){b.get("/api/history/gameweek").success(function(b){a.model=b})}]),angular.module("frontendApp").controller("GameweekscoresCtrl",["$scope","$http","$routeParams",function(a,b,c){var d="/api/history/gameweek/"+c.gameWeekNo;b.get(d).success(function(b){a.model=b})}]),angular.module("frontendApp").controller("FixtureCtrl",["$scope","$http","$routeParams",function(a,b,c){var d="/api/fixture/"+c.fxid;b.get(d).success(function(b){a.model=b});var e="/api/fixturepredictiongraph/"+c.fxid;b.get(e).success(function(b){a.labels=b.labels,a.data=b.data,a.options={animationSteps:10,animateRotate:!1,animateScale:!0}})}]),angular.module("frontendApp").controller("EditpredictionsCtrl",["$scope","$http",function(a,b){b.get("/api/editpredictions").success(function(b){a.model=b}),a.submitResult=function(a){var c={predictionId:a.predictionId,score:a.score};b.post("/api/editprediction",c).success(function(){a.submitted=!0})}}]),angular.module("frontendApp").controller("HistorybymonthCtrl",["$scope","$http","$routeParams",function(a,b){var c="/api/history/month";b.get(c).success(function(b){a.model=b})}]),angular.module("frontendApp").controller("HistorybymonthwithmonthCtrl",["$scope","$http","$routeParams",function(a,b,c){var d="/api/history/month/"+c.month;b.get(d).success(function(b){a.model=b})}]),angular.module("frontendApp").controller("HistorybygameweekCtrl",["$scope","$http",function(a,b){var c="/api/history/gameweek";b.get(c).success(function(b){a.model=b})}]),angular.module("frontendApp").controller("HistorybygameweekwithgameweekCtrl",["$scope","$http","$routeParams",function(a,b,c){var d="/api/history/gameweek/"+c.gameweekno;b.get(d).success(function(b){a.model=b})}]),angular.module("frontendApp").controller("AdminaddresultsforgameweekCtrl",["$scope","$http","$routeParams",function(a,b,c){var d="/api/admin/getclosedfixturesforgameweek/"+c.gameweekno;b.get(d).success(function(b){a.model=b})}]);