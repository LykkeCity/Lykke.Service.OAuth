var app = angular.module('registerApp');

app.directive('strongPassword', ['$q', 'registerService', function($q, registerService) {
    return {
        require: 'ngModel',
        link: function(scope, elm, attrs, ctrl) {
            ctrl.$asyncValidators.strongPassword = function(modelValue, viewValue) {
                if (ctrl.$isEmpty(modelValue)) {
                    return $q.resolve();
                }

                var def = $q.defer();

                registerService.checkPassword(modelValue).then(function (result) {
                    if (result)
                        def.resolve();
                    else
                        def.reject();
                });

                return def.promise;
            };
        }
    };
}]);
