(function () {
    'use strict';

    angular.module('app').controller('pinController', pinController);

    pinController.$inject = ['signupService'];

    function pinController(signupService) {
        var vm = this;

        function handleLogout() {
            signupService.signOut();
        }

        vm.handlers = {
            handleLogout: handleLogout
        };
    }
})();
