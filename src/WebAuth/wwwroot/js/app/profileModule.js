(function () {
    'use strict';

    angular.module('profileApp', ['ngFileUpload', 'ngImgCrop']).run(function ($http) {
        $http.defaults.headers.common['RequestVerificationToken'] =
            angular.element('input[name="__RequestVerificationToken"]').attr('value');

        $('#profile-page').show();
    });

})();
