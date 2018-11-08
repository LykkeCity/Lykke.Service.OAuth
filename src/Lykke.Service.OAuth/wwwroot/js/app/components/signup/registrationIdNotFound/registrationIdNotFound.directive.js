(function () {
    'use strict';

    var app = angular.module('app');

    app
    .directive('registrationIdNotFound', function () {

        return {
            restrict: 'E',
            scope: {},
            templateUrl: '/js/app/components/signup/registrationIdNotFound/registrationIdNotFound.template.html'
        }
    });
})();
