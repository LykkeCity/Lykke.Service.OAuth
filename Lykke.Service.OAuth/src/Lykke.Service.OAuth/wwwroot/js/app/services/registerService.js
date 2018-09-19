(function () {
    'use strict';

    angular
        .module('registerApp')
        .service('registerService', registerService);

    registerService.$inject = ['$http'];

    function registerService($http) {

        function verifyEmail(key, code) {
            return $http.post('/signup/verifyEmail', {key: key, code: code})
                .then(function (data) {
                    return data.data;
                });
        }

        function resendCode(key, captcha) {
            return $http.post('/signup/resendCode', {key: key,  captcha: captcha})
                .then(function (data) {
                    return data.data;
                });
        }

        function register(model) {
            return $http.post('/signup/complete', model)
                .then(function (data) {
                    return data.data;
                });
        }

        function checkPassword(password) {
            return $http.post('/signup/checkPassword', '\''+ password + '\'')
                .then(function (data) {
                    return data.data;
                });
        }

        return {
            verifyEmail: verifyEmail,
            checkPassword: checkPassword,
            resendCode: resendCode,
            register: register
        }
    }
})();
