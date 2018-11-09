(function () {
    'use strict';

    angular.module('app').controller('signupController', signupController);

    signupController.$inject = ['signupService', '$scope'];

    function signupController(signupService, $scope) {
        var vm = this;

        $scope.$on('currentStepChanged', function (event, currentStep) {
            vm.data.currentStep = currentStep;
        });

        signupService.init().then(function () {
            vm.data = {
                currentStep: signupService.getRegistrationStep(),
                errorCode: signupService.getErrorCode(),
            };
        });
    }
})();
