(function () {
    'use strict';

    angular.module('app').controller('signupController', signupController);

    signupController.$inject = ['signupService', '$scope', 'page'];

    function signupController(signupService, $scope, page) {
        var vm = this;

        $scope.$on('currentStepChanged', function (event, currentStep) {
            vm.data.currentStep = currentStep;
        });

        $scope.$on('currentPageChanged', function (event, currentPage) {
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
