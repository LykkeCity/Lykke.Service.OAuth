(function () {
    'use strict';

    var app = angular.module('app');

    app
    .directive('pin', function () {

        return {
            restrict: 'E',
            scope: {},
            templateUrl: '/js/app/components/signup/pin/pin.template.html'
        }
    });
})();
