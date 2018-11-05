(function () {
    'use strict';

    var app = angular.module('app');

    app.directive('strongPassword', function () {
        return {
            require: 'ngModel',
            link: function(scope, elm, attrs, ctrl) {
                ctrl.$validators.strongPassword = function (modelValue) {
                    var passwordRegex = /^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,}$/;

                    return !modelValue || passwordRegex.test(modelValue);
                };
            }
        };
    });
})();

