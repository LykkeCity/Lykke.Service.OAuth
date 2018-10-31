(function () {
    'use strict';

    angular.module('registerApp', ['vcRecaptcha', 'ui.mask'])
        .factory('responseToCamelCaseInterceptor', [function () {  
            return {
                response: function (response) {
                    function transformObjectKeysToCamelCase(obj) {
                        Object.keys(obj).forEach(function (key) {
                            if (angular.isObject(obj[key])) {
                                transformObjectKeysToCamelCase(obj[key]);
                            }
                            var newKey = key.charAt(0).toLowerCase() + key.substr(1);
                            if (key !== newKey) {
                                Object.defineProperty(obj, newKey, Object.getOwnPropertyDescriptor(obj, key));
                                delete obj[key];
                            }
                        });
                    }

                    if (response && response.data && angular.isObject(response.data)) {
                        transformObjectKeysToCamelCase(response.data);
                    }

                    return response;
                }
            }
        }])
        .config(['$httpProvider', function ($httpProvider) {  
            $httpProvider.interceptors.push('responseToCamelCaseInterceptor');
        }])
        .run(function ($http) {
            $http.defaults.headers.common['RequestVerificationToken'] =
                angular.element('input[name="__RequestVerificationToken"]').attr('value');
        });

})();
