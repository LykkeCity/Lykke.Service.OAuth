var app = angular.module('registerApp');

app.directive('affiliateCode', ['$q', 'registerService', function($q, registerService) {
    return {
        require: 'ngModel',
        link: function(scope, elm, attrs, ctrl) {
            ctrl.$asyncValidators.affiliateCode = function(modelValue, viewValue) {
                if (ctrl.$isEmpty(modelValue)) {
                    return $q.resolve();
                }

                var def = $q.defer();

                registerService.checkAffiliateCode(modelValue).then(function (result) {
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
