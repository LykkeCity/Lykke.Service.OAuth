(function () {
    'use strict';

    angular.module('app').controller('signupController', signupController);

    signupController.$inject = ['signupService', '$timeout'];

    function signupController(signupService, $timeout) {
        var vm = this;

        function handleCarouselLoaded() {
            if (!vm.data.loaded) {
                $timeout(function () {
                    vm.data.loaded = true;
                    window.dispatchEvent(new Event('resize'));
                });
            }
        }

        vm.data = {
            loaded: false,
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
            handleCarouselLoaded: handleCarouselLoaded
        };
    }
})();
