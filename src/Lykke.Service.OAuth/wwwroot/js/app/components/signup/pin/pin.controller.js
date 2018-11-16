(function () {
    'use strict';

    angular.module('app').controller('pinController', pinController);

    pinController.$inject = ['signupService', '$scope', 'page', 'signupStep', 'signupEvent'];

    function pinController(signupService, $scope, page, signupStep, signupEvent) {
        var vm = this;

        function handleLogout() {
            signupService.signOut();
            $scope.$emit(signupEvent.currentStepChanged, signupStep.initialInfo);
            $scope.$emit(signupEvent.currentPageChanged, page.signIn);
        }

        vm.handlers = {
            handleLogout: handleLogout
        };
    }
})();
