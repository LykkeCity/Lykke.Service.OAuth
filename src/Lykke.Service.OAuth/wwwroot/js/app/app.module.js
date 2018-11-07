(function () {
    'use strict';

    angular.module('app', ['ui.carousel'])
        .config(['$locationProvider', function ($locationProvider) {
            $locationProvider.html5Mode({
                enabled: true,
                requireBase: false
            });
        }])
        .run(function () {

        });
})();
