(function () {
    'use strict';

    var app = angular.module('app');

    app
    .controller('inputPasswordController', ['$scope', function ($scope) {
        function toggle() {
            $scope.isHidden = !$scope.isHidden;
        }

        $scope.toggle = toggle;
        $scope.isHidden = true;
    }])
    .directive('inputPassword', function () {
        return {
            restrict: 'E',
            scope: {
                ngModel: '=',
                placeholder: '@'
            },
            template: `
                <div data-ng-controller="inputPasswordController" class="lykke-input-password">
                    <input placeholder="{{placeholder}}" data-ng-show="isHidden" class="lykke-input" type="password" data-ng-model="ngModel">
                    <input placeholder="{{placeholder}}" data-ng-hide="isHidden" class="lykke-input" type="text" data-ng-model="ngModel">
                    <div class="lykke-input-password__toggle" data-ng-click="toggle()">
                        <img data-ng-show="isHidden" src="/images/show-icn.svg" />
                        <img data-ng-hide="isHidden" src="/images/hide-icn.svg" />
                    </div>
                </div>
            `
        }
    });
})();
