(function () {
    'use strict';

    var app = angular.module('app');

    app
    .directive('inputPassword', ['inputPasswordController', function (inputPasswordController) {
        return {
            restrict: 'E',
            scope: {
                ngModel: '=',
                placeholder: '@',
                name: '@'
            },
            bindToController: true,
            controllerAs: 'vm',
            controller: inputPasswordController,
            templateUrl: '/js/app/components/shared/inputPassword/inputPassword.template.html'
        }
    }]);
})();
