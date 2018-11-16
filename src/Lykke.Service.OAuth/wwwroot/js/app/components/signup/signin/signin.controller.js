(function () {
    'use strict';

    angular.module('app').controller('signInController', signInController);

    signInController.$inject = ['$scope', 'page'];

    function signInController($scope, page) {
        var vm = this;

        function handleCarouselLoaded() {
            vm.data.loaded = true;
        }

        function handleSignUpClick() {
            $scope.$emit('currentPageChanged', page.signUp);
        }

        vm.data = {
            loaded: false,
            model: {
                email: '',
                password: ''
            }
        };

        vm.handlers = {
            handleSignUpClick: handleSignUpClick,
            handleCarouselLoaded: handleCarouselLoaded
        };
    }
})();
