(function () {
    'use strict';

    angular
        .module('app')
        .service('envService', envService);

    envService.$inject = ['$window'];

    function envService($window) {

        function getFundsUrl() {
            var fundsUrls = {
                'localhost:5000': 'http://localhost:3000',
                'auth.lykke.com': 'http://wallet.lykke.com',
                'auth-dev.lykkex.net': 'http://webwallet-dev.lykkex.net',
                'auth-dev-custom.lykkex.net': 'http://webwallet-dev.lykkex.net',
                'auth-test.lykkex.net': 'http://webwallet-test.lykkex.net'
            }

            return fundsUrls[$window.location.host];
        }

        return {
            getFundsUrl: getFundsUrl
        };
    }
})();
