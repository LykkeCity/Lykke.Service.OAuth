(function () {
    'use strict';

    angular.module('registerApp').controller('registrationController', registrationController);

    registrationController.$inject = ['registerService'];

    function registrationController(registerService) {
        var vm = this;

        vm.data = {
            step: 1,
            loading: false,
            model: {},
            step1Form: {
                code: null,
                result: true,
                isEmailTaken: false,
                resendingCode: false,
                resendCount: 0
            },
            step2Form: {
                showPassword: false,
                showConfirmPassword: false
            },
            step3Form: {},
            summaryErrors: []
        };

        vm.handlers = {
            goToLogin: goToLogin,
            verifyEmail: verifyEmail,
            resendCode: resendCode,
            isStep2FormSubmitDisabled: isStep2FormSubmitDisabled,
            setPassword: setPassword,
            register: register
        };

        vm.init = function(key, email, resendCount) {
            vm.data.key = key;
            vm.data.email = email;
            vm.data.step1Form.resendCount = resendCount;
        };

        function verifyEmail() {
            vm.data.loading = true;
            registerService.verifyEmail(vm.data.key, vm.data.step1Form.code).then(function (result) {
                if (result.code !== null) {
                    vm.data.model.returnUrl = result.code.returnUrl;
                    vm.data.model.referer = result.code.referer;
                    vm.data.model.email = result.code.email;
                    vm.data.step1Form.isEmailTaken = result.isEmailTaken;
                    vm.data.step1Form.result = true;

                    if (!result.isEmailTaken) {
                        vm.data.step = 2;
                    }
                } else {
                    vm.data.step1Form.result = false;
                }

                vm.data.loading = false;
            });
        }

        function resendCode() {
            if (vm.data.step1Form.resendingCode || vm.data.step1Form.resendCount >= 2)
                return;

            vm.data.step1Form.resendingCode = true;
            registerService.resendCode(vm.data.key).then(function (result) {
                $.notify({ title: 'Code successfully sent!' }, { className: 'success' });
                vm.data.step1Form.resendingCode = false;
                vm.data.step1Form.resendCount++;
            });
        }

        function isStep2FormSubmitDisabled() {
            return !vm.step2Form.$valid ||
                (!vm.data.step2Form.password.length || !vm.data.step2Form.confirmPassword.length) ||
                vm.data.step2Form.password !== vm.data.step2Form.confirmPassword;
        }

        function setPassword() {
            vm.data.model.password = vm.data.step2Form.password;
            vm.data.step = 3;
        }

        function register() {
            vm.data.model.firstName = vm.data.step3Form.firstName;
            vm.data.model.lastName = vm.data.step3Form.lastName;
            vm.data.loading = true;
            registerService.register(vm.data.model).then(function (result) {
                if (result.errors.length) {
                    vm.data.summaryErrors = result.errors;
                    vm.data.loading = false;
                }
                else {
                    window.location = vm.data.model.returnUrl ? vm.data.model.returnUrl : '/';
                }
            });

        }

        function goToLogin() {
            window.location = '/signin';
        }
    }
})();
