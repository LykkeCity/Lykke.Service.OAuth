(function () {
    'use strict';

    angular.module('app').controller('initialInfoController', initialInfoController);

    initialInfoController.$inject = ['signupService', 'signupStep', '$timeout', '$window', '$scope'];

    function initialInfoController(signupService, signupStep, $timeout, $window, $scope) {
        var vm = this;
        var carouselReInitPromise;

        function reInitCarousel(event) {
            if (!event.detail) {
                $timeout.cancel(carouselReInitPromise);
                carouselReInitPromise = $timeout(function () {
                    window.dispatchEvent(new CustomEvent('resize', { detail: 'skipReInit' }));
                }, vm.data.carouselSpeed * 2);
            }
        }

        angular.element($window).on('resize', reInitCarousel);
        function handleCarouselLoaded() {
            if (vm.data.isCarouselRendered) {
                vm.data.loaded = true;
            } else {
                $timeout(function () {
                    vm.data.isCarouselRendered = true;
                    window.dispatchEvent(new CustomEvent('resize', { detail: 'skipReInit' }));
                });
            }
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

        vm.data = {
            isCarouselRendered: false,
            carouselSpeed: 300,
            loaded: false,
            isSubmitting: false,
            isPasswordPwned: false,
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
    }
})();
