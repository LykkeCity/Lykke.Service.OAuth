(function () {
    'use strict';

    angular.module('app').controller('signupController', signupController);

    signupController.$inject = ['signupService', 'signupStep', '$timeout', '$q'];

    function signupController(signupService, signupStep, $timeout, $q) {
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

        function validateEmail() {
            var deferred = $q.defer();
            var bcrypt = dcodeIO.bcrypt;
            bcrypt.hash(vm.data.model.email, vm.data.bCryptWorkFactor, function (err, hash) {
                if (err) {
                    deferred.reject();
                }

                signupService.checkEmailTaken(vm.data.model.email, hash).then(function (data) {
                    vm.data.registrationId = data.registrationId;
                    deferred.resolve(data.isEmailTaken);
                })
            });
            return deferred.promise;
        }

        function handleSubmit() {
            if (!vm.data.isSubmitting) {
                vm.data.isSubmitting = true;
                vm.data.isEmailTaken = false;

                validateEmail().then(function (isEmailTaken) {
                    if (isEmailTaken) {
                        vm.data.isSubmitting = false;
                        vm.data.isEmailTaken = isEmailTaken;
                        return;
                    }

                    signupService.sendInitialInfo(
                        vm.data.model.email,
                        vm.data.model.password,
                        vm.data.registrationId
                    ).then(function (data) {
                        vm.data.currentStep = signupStep.accountInformation;
                    });
                });
            }
        }

        vm.data = {
            steps: signupStep,
            bCryptWorkFactor: 0,
            isCarouselRendered: false,
            loaded: false,
            isSubmitting: false,
            isEmailTaken: false,
            currentStep: signupStep.initialInfo,
            registrationId: '',
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
            ],
            countries: []
        };

        vm.handlers = {
            handleCarouselLoaded: handleCarouselLoaded,
            handleSubmit: handleSubmit
        };

        signupService.getSettings().then(function (data) {
            vm.data.bCryptWorkFactor = data.bCryptWorkFactor;
        });
    }
})();
