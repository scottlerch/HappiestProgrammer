var app = angular.module("app", []);

var urlConstant = 'api/languages?date=';
var url = urlConstant.concat('2014-01-13');

app.directive('repeatDone', function () {
    return function (scope, element, attrs) {
        if (scope.$last) { // all are rendered
            setTimeout(function () { scope.$eval(attrs.repeatDone); }, 0);
        }
    }
});

app.factory('languageFactory', ["$http", function ($http) {
    return {
        getLanguages: function () {
            return $http.get(url);
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
    return dateParts[2].concat('-', dateParts[0], '-', dateParts[1])
};

app.controller("HomeCtrl", ["$scope", "languageFactory", "notificationFactory", function ($scope, languageFactory, notificationFactory) {
    $scope.languages = [];

    var d = new Date();
    d.setDate(d.getDate() - 1);

    $scope.rankSelection = { date: getDateString(d) };

    url = urlConstant.concat(getDateParamter($scope.rankSelection.date));

    $scope.updateRankDate = function () {
        // HACK: since angular isn't working well with bootstrap-datepicker
        $scope.rankSelection.date = document.getElementById('rankDate').value;
        url = urlConstant.concat(getDateParamter($scope.rankSelection.date));
        languageFactory.getLanguages().success(getLanguagesSuccessCallback).error(errorCallback);
    };

    $scope.layoutDone = function () {
        $("[rel=tooltip]").tooltip({ placement: 'left' });
    };

    var getLanguagesSuccessCallback = function (data, status) {
        $scope.languages = data;
    };

    var successCallback = function (data, status, headers, config) {
        notificationFactory.success();

        return languageFactory.getLanguages().success(getLanguagesSuccessCallback).error(errorCallback);
    };

    var errorCallback = function (data, status, headers, config) {
        notificationFactory.error(data.ExceptionMessage);
    };

    languageFactory.getLanguages().success(getLanguagesSuccessCallback).error(errorCallback);
}]);


$(document).ready(function () {
    $("[rel=tooltip]").tooltip({ placement: 'left' });
});