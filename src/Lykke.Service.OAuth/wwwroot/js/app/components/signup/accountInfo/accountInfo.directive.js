(function () {
    'use strict';

    var app = angular.module('app');

    app
    .directive('accountInfo', function () {

        return {
            restrict: 'E',
            scope: {},
            templateUrl: '/js/app/components/signup/accountInfo/accountInfo.template.html'
        }
    });
})();
