(function () {
    'use strict';

    var app = angular.module('app');

    app
    .constant('inputPasswordController', function () {
        var vm = this;

        function toggle() {
            vm.isHidden = !vm.isHidden;
        }

        vm.toggle = toggle;
        vm.isHidden = true;
    });
})();
