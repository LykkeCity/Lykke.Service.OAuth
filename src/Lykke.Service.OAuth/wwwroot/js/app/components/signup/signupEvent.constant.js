(function () {
    'use strict';

    var app = angular.module('app');

    app.constant('signupEvent', {
        currentStepChanged: 'CurrentStepChanged',
        currentPageChanged: 'CurrentPageChanged'
    });
})();
