(function () {
    'use strict';

    angular
        .module('app')
        .service('signupService', signupService);

    signupService.$inject = ['$http', 'env', '$q', '$window', '$location', 'signupStep'];

    function signupService($http, env, $q, $window, $location, signupStep) {
        var bCryptWorkFactor;
        var verifiedEmailIds = {};
        var registrationStep = signupStep.initialInfo;

        function init() {
            var deferred = $q.defer();

            getSettings().then(function (data) {
                bCryptWorkFactor = data.bCryptWorkFactor;
            });

            var registrationId = $location.search().registrationId || $window.localStorage.getItem('registrationId');
            if (registrationId) {
                saveRegistrationId(registrationId);

                return getStatus(registrationId).then(function (response) {
                    registrationStep = response;
                }).catch(function () {
                    registrationStep = signupStep.registrationIdNotFound;
                }).finally(function () {
                    deferred.resolve();
                });
            } else {
                deferred.resolve();
            }

            return deferred.promise;
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
                    saveRegistrationId(response.data.registrationId);
                    return response.data;
                });
        }

        function saveRegistrationId(registrationId) {
            $window.localStorage.setItem('registrationId', registrationId);
        }

        function getRegistrationStep() {
            return registrationStep;
        }

        function getStatus(registrationId) {
            var statusUrl = '/api/registration/status/' + registrationId;

            return $http
                .get(statusUrl)
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

        function signOut() {
            $window.localStorage.removeItem('registrationId');
            $window.location.replace($location.path());
        }

        return {
            init: init,
            checkEmailTaken: checkEmailTaken,
            validateEmail: validateEmail,
            sendInitialInfo: sendInitialInfo,
            getSettings: getSettings,
            signOut: signOut,
            getRegistrationStep: getRegistrationStep
        };
    }
})();
