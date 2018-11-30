(function () {
    'use strict';

    angular.module('app').controller('initialInfoController', initialInfoController);

    initialInfoController.$inject = ['$window', 'signupService', 'signupStep', '$scope', 'page', 'signupEvent'];

    function initialInfoController($window, signupService, signupStep, $scope, page, signupEvent) {
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
                    var tracker = $window.ga.getAll()[0];
                    var cid = tracker.get('clientId');

                    signupService.sendInitialInfo(
                        vm.data.model.email,
                        vm.data.model.password,
                        cid
                    ).then(function (data) {
                        vm.data.isSubmitting = false;
                        $scope.$emit(signupEvent.currentStepChanged, signupStep.accountInformation);
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
            $scope.$emit(signupEvent.currentPageChanged, page.signIn);
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
