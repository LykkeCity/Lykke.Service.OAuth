(function () {
    'use strict';

    angular
        .module('app')
        .service('signupService', signupService);

    signupService.$inject = ['$http', 'env'];

    function signupService($http, env) {
        function checkEmailTaken(email, hash) {
            return $http
                .post('/api/registration/email', {email: email, hash: hash})
                .then(function (response) {
                    return response.data;
                });
        }

        function sendInitialInfo(email, password, registrationId) {
            return $http
                .post('/api/registration/initialInfo', {
                    email: email,
                    password: password,
                    registrationId: registrationId,
                    clientId: env.clientId
                })
                .then(function (response) {
                    return response.data;
                });
        }

        function getSettings() {
            return $http
                .get('/api/settings/registration')
                .then(function (response) {
                    return response.data;
                });
        }

        return {
            checkEmailTaken: checkEmailTaken,
            sendInitialInfo: sendInitialInfo,
            getSettings: getSettings
        };
    }
})();
