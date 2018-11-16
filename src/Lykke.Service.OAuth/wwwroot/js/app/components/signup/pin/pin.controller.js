(function () {
    'use strict';

    angular.module('app').controller('pinController', pinController);

    pinController.$inject = ['signupService', '$scope', 'page', 'signupStep'];

    function pinController(signupService, $scope, page, signupStep) {
        var vm = this;

        function handleLogout() {
            signupService.signOut();
            $scope.$emit('currentStepChanged', signupStep.initialInfo);
            $scope.$emit('currentPageChanged', page.signIn);
        }

        vm.handlers = {
            handleLogout: handleLogout
        };
    }
})();
