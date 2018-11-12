(function () {
    'use strict';

    angular.module('app').controller('registrationIdNotFoundController', registrationIdNotFoundController);

    registrationIdNotFoundController.$inject = ['signupService'];

    function registrationIdNotFoundController(signupService) {
        var vm = this;

        function handleContinue() {
            signupService.signOut();
        }

        vm.handlers = {
            handleContinue: handleContinue
        };
    }
})();
