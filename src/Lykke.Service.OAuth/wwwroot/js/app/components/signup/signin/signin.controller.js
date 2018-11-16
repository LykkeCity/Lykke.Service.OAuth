(function () {
    'use strict';

    angular.module('app').controller('signInController', signInController);

    signInController.$inject = ['$scope', 'page', 'signupEvent'];

    function signInController($scope, page, signupEvent) {
        var vm = this;

        function handleCarouselLoaded() {
            vm.data.loaded = true;
        }

        function handleSignUpClick() {
            $scope.$emit(signupEvent.currentPageChanged, page.signUp);
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
