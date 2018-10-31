(function () {
    'use strict';

    angular
        .module('app')
        .service('signupService', signupService);

    signupService.$inject = ['$http'];

    function signupService() {
        // TODO: Add API calls
        return {};
    }
})();
