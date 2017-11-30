(function () {
    'use strict';

    var app = angular.module('registerApp', [])
        .run(function ($http) {
            $http.defaults.headers.common['RequestVerificationToken'] =
                angular.element('input[name="__RequestVerificationToken"]').attr('value');
        });

})();
