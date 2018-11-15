(function () {
    'use strict';

    var app = angular.module('app');

    app.constant('phoneErrorCode', {
        phoneNumberInUse: 'PhoneNumberInUse',
        invalidPhoneFormat: 'InvalidPhoneFormat'
    });
})();
