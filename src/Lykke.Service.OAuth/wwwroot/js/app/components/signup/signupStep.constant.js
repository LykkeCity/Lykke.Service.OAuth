(function () {
    'use strict';

    var app = angular.module('app');

    app.constant('signupStep', {
        initialInfo: 'InitialInfo',
        accountInformation: 'AccountInformation',
        pin: 'Pin'
    });
})();
