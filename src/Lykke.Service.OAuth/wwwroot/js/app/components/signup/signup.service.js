(function () {
    'use strict';

    angular
        .module('app')
        .service('signupService', signupService);

    signupService.$inject = ['$http', 'env', '$q'];

    function signupService($http, env, $q) {
        var bCryptWorkFactor;
        var verifiedEmailIds = {};

        function init() {
            getSettings().then(function (data) {
                bCryptWorkFactor = data.bCryptWorkFactor;
            });
        }

        function validateEmail(email) {
            var deferred = $q.defer();

            if (angular.isDefined(verifiedEmailIds[email])) {
                var isEmailTaken = !verifiedEmailIds[email];
                deferred.resolve(isEmailTaken);
                return deferred.promise;
            }

            var bcrypt = dcodeIO.bcrypt;
            bcrypt.hash(email, bCryptWorkFactor, function (err, hash) {
                if (err) {
                    deferred.reject();
                }

                checkEmailTaken(email, hash).then(function (data) {
                    verifiedEmailIds[email] = data.registrationId;
                    deferred.resolve(data.isEmailTaken);
                })
            });
            return deferred.promise;
        }

        function checkEmailTaken(email, hash) {
            return $http
                .post('/api/registration/email', {email: email, hash: hash})
                .then(function (response) {
                    return response.data;
                });
        }

        function sendInitialInfo(email, password) {
            return $http
                .post('/api/registration/initialInfo', {
                    email: email,
                    password: password,
                    registrationId: verifiedEmailIds[email],
                    clientId: env.clientId
                })
                .then(function (response) {
                    return response.data;
                });
        }

        function getRegistrationId(email) {
            return verifiedEmailIds[email];
        }

        function getSettings() {
            return $http
                .get('/api/settings/registration')
                .then(function (response) {
                    return response.data;
                });
        }

        return {
            init: init,
            checkEmailTaken: checkEmailTaken,
            validateEmail: validateEmail,
            sendInitialInfo: sendInitialInfo,
            getSettings: getSettings,
            getRegistrationId: getRegistrationId
        };
    }
})();
