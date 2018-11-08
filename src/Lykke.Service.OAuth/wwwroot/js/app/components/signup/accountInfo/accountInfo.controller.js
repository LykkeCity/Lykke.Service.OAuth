(function () {
    'use strict';

    angular.module('app').controller('accountInfoController', accountInfoController);

    accountInfoController.$inject = ['signupService'];

    function accountInfoController(signupService) {
        var vm = this;

        function handleLogout() {
            signupService.signOut();
        }

        vm.handlers = {
            handleLogout: handleLogout
        };
    }
})();
