(function () {
    'use strict';

    angular.module('app', ['ui.carousel', 'ui.select', 'ngSanitize', 'ang-dialogs'])
        .config(['$locationProvider', function ($locationProvider) {
            $locationProvider.html5Mode({
                enabled: true,
                requireBase: false
            });
        }])
        .run(function () {

        });
})();
