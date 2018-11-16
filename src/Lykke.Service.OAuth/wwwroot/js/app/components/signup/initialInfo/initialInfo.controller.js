(function () {
    'use strict';

    angular.module('app').controller('initialInfoController', initialInfoController);

    initialInfoController.$inject = ['signupService', 'signupStep', '$scope', 'page'];

    function initialInfoController(signupService, signupStep, $scope, page) {
        var vm = this;

        function handleCarouselLoaded() {
            vm.data.loaded = true;
        }

        function handleSubmit(form) {
            if (form.$pending) {
                vm.data.isSubmitting = true;
                var pendingWatch = $scope.$watch(function () {
                    return form.$pending;
                }, function (pending) {
                    if (!pending) {
                        pendingWatch();
                        handleSubmit(form);
                    }
                });
            }
            if (form.$valid) {
                if (!vm.data.isSubmitting) {
                    vm.data.isSubmitting = true;

                    signupService.sendInitialInfo(
                        vm.data.model.email,
                        vm.data.model.password
                    ).then(function (data) {
                        vm.data.isSubmitting = false;
                        $scope.$emit('currentStepChanged', signupStep.accountInformation);
                    }).catch(function (error) {
                        var passwordIsPwnedError = 'PasswordIsPwned';
                        vm.data.isSubmitting = false;
                        vm.data.isPasswordPwned = passwordIsPwnedError === error.data.error;
                    });
                }
            } else {
                vm.data.isSubmitting = false;
            }
        }

        function handleEmailKeydown(form) {
            form.$setPristine();
            form.email.$setUntouched();
        }

        function handlePasswordKeydown() {
            vm.data.isPasswordPwned = false;
        }

        function handleSignInClick() {
            $scope.$emit('currentPageChanged', page.signIn);
        }

        vm.data = {
            loaded: false,
            isSubmitting: false,
            isPasswordPwned: false,
            model: {
                email: '',
                password: ''
            }
        };

        vm.handlers = {
            handleSignInClick: handleSignInClick,
            handleCarouselLoaded: handleCarouselLoaded,
            handleSubmit: handleSubmit,
            handleEmailKeydown: handleEmailKeydown,
            handlePasswordKeydown: handlePasswordKeydown
        };
    }
})();
