(function () {
    'use strict';

    angular.module('app').controller('signupController', signupController);

    signupController.$inject = ['signupService', 'signupStep', '$timeout'];

    function signupController(signupService, signupStep, $timeout) {
        var vm = this;

        function handleCarouselLoaded() {
            if (vm.data.isCarouselRendered) {
                vm.data.loaded = true;
            } else {
                $timeout(function () {
                    vm.data.isCarouselRendered = true;
                    window.dispatchEvent(new Event('resize'));
                });
            }
        }

        function handleSubmit() {
            if (!vm.data.isSubmitting) {
                vm.data.isSubmitting = true;

                signupService.sendInitialInfo(
                    vm.data.model.email,
                    vm.data.model.password
                ).then(function (data) {
                    vm.data.isSubmitting = false;
                    vm.data.currentStep = signupStep.accountInformation;
                }).catch(function (error) {
                    var passwordIsPwnedError = 'PasswordIsPwned';
                    vm.data.isSubmitting = false;
                    vm.data.isPasswordPwned = passwordIsPwnedError === error.data.error;
                });
            }
        }

        function handleEmailKeydown(form) {
            form.$setPristine();
            form.email.$setUntouched();
        }

        function handlePasswordKeydown() {
            vm.data.isPasswordPwned = false;
        }

        vm.data = {
            isCarouselRendered: false,
            loaded: false,
            isSubmitting: false,
            isPasswordPwned: false,
            currentStep: signupStep.initialInfo,
            model: {
                email: '',
                password: ''
            },
            slides: [
                {
                    imageSrc: '/images/carousel/rectangle-2.png',
                    text: 'I was recommended Lykke from a good friend of mine. App is great. User friendly. Service is fast. Very responsive.'
                },
                {
                    imageSrc: '/images/carousel/rectangle-3.png',
                    text: 'Thank-you very much for your support... The service by Lykke is incredible. 👍🏻'
                }
            ]
        };

        vm.handlers = {
            handleCarouselLoaded: handleCarouselLoaded,
            handleSubmit: handleSubmit,
            handleEmailKeydown: handleEmailKeydown,
            handlePasswordKeydown: handlePasswordKeydown
        };

        signupService.init();
    }
})();
