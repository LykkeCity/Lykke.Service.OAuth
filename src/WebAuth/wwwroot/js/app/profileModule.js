(function () {
    'use strict';

    var app = angular.module('profileApp', ['ngFileUpload']).run(function ($http) {
        $http.defaults.headers.common['RequestVerificationToken'] =
            angular.element('input[name="__RequestVerificationToken"]').attr('value');

        $('#profile-page').show();
    });

})();