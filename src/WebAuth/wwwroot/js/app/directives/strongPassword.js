var app = angular.module('registerApp');

app.directive('strongPassword', function() {
    return {
        require: 'ngModel',
        link: function(scope, elm, attrs, ctrl) {
            ctrl.$validators.strongPassword = function(modelValue, viewValue) {
                if (ctrl.$isEmpty(modelValue)) {
                    return true;
                }

                if (modelValue.length < 8)
                    return false;

                var hasUpperCase = /[A-Z]/.test(modelValue);
                var hasLowerCase = /[a-z]/.test(modelValue);
                var hasNumbers = /\d/.test(modelValue);
                return hasUpperCase && hasLowerCase && hasNumbers;
            };
        }
    };
});
