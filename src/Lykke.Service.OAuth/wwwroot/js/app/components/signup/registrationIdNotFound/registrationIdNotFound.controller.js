(function () {
    'use strict';

    angular.module('app').controller('registrationIdNotFoundController', registrationIdNotFoundController);

    registrationIdNotFoundController.$inject = ['signupService', '$window', '$location'];

    function registrationIdNotFoundController(signupService, $window, $location) {
        var vm = this;

        function handleContinue() {
            signupService.signOut();
            $window.location.replace($location.path());
        }

        vm.handlers = {
            handleContinue: handleContinue
        };
    }
})();
