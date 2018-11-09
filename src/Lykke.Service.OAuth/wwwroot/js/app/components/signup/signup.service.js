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
        var currentErrorCode = null;

        function init() {
            var deferred = $q.defer();

            getSettings().then(function (data) {
                bCryptWorkFactor = data.bCryptWorkFactor;
            });

            var registrationId = getRegistrationId();
            if (registrationId) {
                saveRegistrationId(registrationId);

                return getStatus(registrationId).then(function (response) {
                    registrationStep = response.registrationStep;
                }).catch(function (error) {
                    registrationStep = null;
                    currentErrorCode = error.data.error;
                }).finally(function () {
                    deferred.resolve();
                });
            } else {
                deferred.resolve();
            }

            return deferred.promise;
        }

        function validateEmail(emailViewModel) {
            var deferred = $q.defer();
            var email = emailViewModel.toLowerCase();

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
                .post('/api/registration/email', {email: email.toLowerCase(), hash: hash})
                .then(function (response) {
                    return response.data;
                });
        }

        function sendInitialInfo(email, password) {
            return $http
                .post('/api/registration/initialInfo', {
                    email: email.toLowerCase(),
                    password: password,
                    registrationId: verifiedEmailIds[email.toLowerCase()],
                    clientId: env.clientId
                })
                .then(function (response) {
                    saveRegistrationId(response.data.registrationId);
                    return response.data;
                });
        }

        function sendAccountInfo(firstName, lastName, countryCodeIso2, phoneNumber) {
            return $http
                .post('/api/registration/accountInfo', {
                    firstName: firstName,
                    lastName: lastName,
                    countryCodeIso2: countryCodeIso2,
                    phoneNumber: phoneNumber,
                    registrationId: getRegistrationId()
                })
                .then(function (response) {
                    saveRegistrationId(response.data.registrationId);
                    return response.data;
                });
        }

        function saveRegistrationId(registrationId) {
            $window.localStorage.setItem('registrationId', registrationId);
        }

        function getRegistrationId() {
            return $location.search().registrationId || $window.localStorage.getItem('registrationId');
        }

        function getRegistrationStep() {
            return registrationStep;
        }

        function getErrorCode() {
            return currentErrorCode;
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

        function getCountries() {
            return $http
                .post('/api/registration/countries', {
                    registrationId: getRegistrationId()
                })
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
            sendAccountInfo: sendAccountInfo,
            getSettings: getSettings,
            signOut: signOut,
            getRegistrationStep: getRegistrationStep,
            getErrorCode: getErrorCode,
            getCountries: getCountries
        };
    }
})();
