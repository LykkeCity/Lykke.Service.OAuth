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
    }]);
})();
