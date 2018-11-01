(function () {
    'use strict';

    var app = angular.module('app');

    app
    .directive('inputPassword', function () {
        return {
            restrict: 'E',
            scope: {
                ngModel: '=',
                placeholder: '@'
            },
            templateUrl: '/js/app/components/shared/inputPassword/inputPassword.template.html'
        }
    });
})();
