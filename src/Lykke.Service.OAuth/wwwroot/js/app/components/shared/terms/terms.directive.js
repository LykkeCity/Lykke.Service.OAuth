(function () {
    'use strict';

    var app = angular.module('app');

    app
    .directive('terms', function () {
        return {
            restrict: 'E',
            templateUrl: '/js/app/components/shared/terms/terms.template.html'
        }
    });
})();
