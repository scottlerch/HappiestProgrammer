var app = angular.module("app", []);

var url = 'api/languages?date=2014-01-13';

app.factory('languageFactory', function ($http) {
    return {
        getLanguages: function () {
            return $http.get(url);
        },
    };
});

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

app.controller("HomeCtrl", function ($scope, languageFactory, notificationFactory) {
    $scope.languages = [];

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
});