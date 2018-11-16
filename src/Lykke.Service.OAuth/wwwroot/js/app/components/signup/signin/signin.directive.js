(function () {
    'use strict';

    var app = angular.module('app');

    app
    .directive('signIn', function () {

        return {
            restrict: 'E',
            scope: {},
            templateUrl: '/js/app/components/signup/signin/signin.template.html'
        }
    });
})();
