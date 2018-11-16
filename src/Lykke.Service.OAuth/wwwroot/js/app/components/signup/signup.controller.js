(function () {
    'use strict';

    angular.module('app').controller('signupController', signupController);

    signupController.$inject = ['signupService', '$scope', 'page', 'signupEvent'];

    function signupController(signupService, $scope, page, signupEvent) {
        var vm = this;

        $scope.$on(signupEvent.currentStepChanged, function (event, currentStep) {
            vm.data.currentStep = currentStep;
        });

        $scope.$on(signupEvent.currentPageChanged, function (event, currentPage) {
            vm.data.currentPage = currentPage;
        });

        signupService.init().then(function () {
            vm.data = {
                currentStep: signupService.getRegistrationStep(),
                errorCode: signupService.getErrorCode(),
                currentPage: page.signUp
            };
        });
    }
})();
