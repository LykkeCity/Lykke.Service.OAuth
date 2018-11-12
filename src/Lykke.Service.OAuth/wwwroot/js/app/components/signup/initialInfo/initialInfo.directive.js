(function () {
    'use strict';

    var app = angular.module('app');

    app
    .directive('initialInfo', function () {

        return {
            restrict: 'E',
            scope: {},
            templateUrl: '/js/app/components/signup/initialInfo/initialInfo.template.html'
        }
    });
})();
