(function () {
    'use strict';

    var app = angular.module('app');

    app
    .directive('inputPassword', function () {
        function inputPasswordController() {
            var vm = this;

            function toggle() {
                vm.isHidden = !vm.isHidden;
            }

            vm.toggle = toggle;
            vm.isHidden = true;
        }

        return {
            restrict: 'E',
            scope: {
                ngModel: '=',
                hasError: '=',
                placeholder: '@',
                name: '@'
            },
            bindToController: true,
            controllerAs: 'vm',
            controller: inputPasswordController,
            templateUrl: '/js/app/components/shared/inputPassword/inputPassword.template.html'
        }
    });
})();
