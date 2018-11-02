(function () {
    'use strict';

    var app = angular.module('app');

    app.directive('emailTaken', function($q, signupService) {
        return {
            require: 'ngModel',
            link: function(scope, elm, attrs, ctrl) {
                ctrl.$asyncValidators.emailTaken = function (modelValue) {
                    if (ctrl.$isEmpty(modelValue)) {
                        return $q.resolve();
                    }

                    var deferred = $q.defer();
                    signupService.validateEmail(modelValue).then(function (isEmailTaken) {
                        if (isEmailTaken) {
                            deferred.reject();
                        } else {
                            deferred.resolve();
                        }
                    });

                    return deferred.promise;
                };
            }
        };
    });
})();

