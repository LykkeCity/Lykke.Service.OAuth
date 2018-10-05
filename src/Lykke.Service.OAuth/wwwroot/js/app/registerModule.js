(function () {
    'use strict';

    angular.module('registerApp', ['vcRecaptcha', 'ui.mask'])
        .run(function ($http) {
            $http.defaults.headers.common['RequestVerificationToken'] =
                angular.element('input[name="__RequestVerificationToken"]').attr('value');
        });

})();
