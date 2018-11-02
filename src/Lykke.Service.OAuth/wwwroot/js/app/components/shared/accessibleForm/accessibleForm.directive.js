(function () {
    'use strict';

    var app = angular.module('app');

    app.directive('accessibleForm', function () {
        return {
            restrict: 'A',
            link: function (scope, elem) {
                elem.on('submit', function () {
                    var firstInvalid = elem[0].querySelector('.ng-invalid');

                    if (firstInvalid) {
                        firstInvalid.focus();
                    }
                });
            }
        };
    });
})();
