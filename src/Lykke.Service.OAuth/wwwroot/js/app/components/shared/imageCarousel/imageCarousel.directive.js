(function () {
    'use strict';

    var app = angular.module('app');

    app
    .directive('imageCarousel', function ($timeout, $window) {
        function imageCarouselController() {
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
                    vm.onLoad();
                } else {
                    $timeout(function () {
                        vm.data.isCarouselRendered = true;
                        window.dispatchEvent(new CustomEvent('resize', { detail: 'skipReInit' }));
                    });
                }
            }

            vm.data = {
                isCarouselRendered: false,
                carouselSpeed: 300,
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
                handleCarouselLoaded: handleCarouselLoaded
            };
        }

        return {
            restrict: 'E',
            scope: {
                onLoad: '='
            },
            bindToController: true,
            controllerAs: 'vm',
            controller: imageCarouselController,
            templateUrl: '/js/app/components/shared/imageCarousel/imageCarousel.template.html'
        }
    });
})();
