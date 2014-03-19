﻿if (!String.prototype.format) {
    String.prototype.format = function () {
        var args = arguments;
        return this.replace(/{(\d+)}/g, function (match, number) {
            return typeof args[number] != 'undefined'
              ? args[number]
              : match
            ;
        });
    };
}

var app = angular.module("app", []);

var languageUrlConstant = 'api/languages?date=';
var languageUrl = languageUrlConstant.concat('2014-01-13');

var commentUrlConstant = 'api/comments?date={0}&language={1}&positive={2}';
var positiveCommentsUrl = commentUrlConstant.format('2014-01-13', 'java', true);

app.directive('repeatDone', function () {
    return function(scope, element, attrs) {
        if (scope.$last) { // all are rendered
            setTimeout(function() { scope.$eval(attrs.repeatDone); }, 0);
        }
    };
});

app.factory('languageFactory', ["$http", function ($http) {
    return {
        getLanguages: function () {
            return $http.get(languageUrl);
        },
        getPositiveComments: function () {
            return $http.get(positiveCommentsUrl);
        },
    };
}]);

app.factory('notificationFactory', function () {

    return {
        success: function () {
            toastr.success("Success");
        },
        error: function (text) {
            toastr.error(text, "Error!");
        }
    };
});

var getDateString = function (d) {
    var dd = d.getDate();
    var mm = d.getMonth() + 1; //January is 0!
    var yyyy = d.getFullYear();
    if (dd < 10) { dd = '0' + dd }
    if (mm < 10) { mm = '0' + mm }
    return mm + '/' + dd + '/' + yyyy;
};

var getDateParamter = function (d) {
    var dateParts = d.split('/');
    return dateParts[2].concat('-', dateParts[0], '-', dateParts[1]);
};

app.controller("HomeCtrl", ["$scope", "languageFactory", "notificationFactory", function ($scope, languageFactory, notificationFactory) {
    $scope.languages = [];
    $scope.positiveComments = [];

    var d = new Date();
    d.setDate(d.getDate() - 1);

    $scope.rankSelection = { date: getDateString(d) };
    $scope.languageSelected = { visible: true, name:"" };

    languageUrl = languageUrlConstant.concat(getDateParamter($scope.rankSelection.date));
    positiveCommentsUrl = commentUrlConstant.format(getDateParamter($scope.rankSelection.date), $scope.languageSelected.name);

    $scope.updateRankDate = function () {
        // HACK: since angular isn't working well with bootstrap-datepicker
        $scope.rankSelection.date = document.getElementById('rankDate').value;

        languageUrl = languageUrlConstant.concat(getDateParamter($scope.rankSelection.date));
        languageFactory.getLanguages().success(getLanguagesSuccessCallback).error(errorCallback);

        positiveCommentsUrl = commentUrlConstant.format(getDateParamter($scope.rankSelection.date), $scope.languageSelected.name, true);
        languageFactory.getPositiveComments().success(getPositiveCommentsSuccessCallback).error(errorCallback);
    };

    $scope.selectLanguage = function (language) {
        $scope.languageSelected.visible = true;
        $scope.languageSelected.name = language;

        positiveCommentsUrl = commentUrlConstant.format(getDateParamter($scope.rankSelection.date), $scope.languageSelected.name, true);
        languageFactory.getPositiveComments().success(getPositiveCommentsSuccessCallback).error(errorCallback);
    };

    $scope.layoutDone = function () {
        $("[rel=tooltip]").tooltip({ placement: 'left' });
    };

    var getLanguagesSuccessCallback = function (data, status) {
        $scope.languages = data;
    };

    var getPositiveCommentsSuccessCallback = function (data, status) {
        $scope.positiveComments = data;
    };

    var errorCallback = function (data, status, headers, config) {
        notificationFactory.error(data.ExceptionMessage);
    };

    languageFactory.getLanguages().success(getLanguagesSuccessCallback).error(errorCallback);
}]);


$(document).ready(function () {
    $("[rel=tooltip]").tooltip({ placement: 'left' });
});