(function () {
    'use strict';

    angular.module('registerApp').controller('registrationController', registrationController);

    registrationController.$inject = ['registerService', 'vcRecaptchaService'];

    function registrationController(registerService, vcRecaptchaService) {
        var vm = this;

        vm.data = {
            step: 1,
            loading: false,
            showResendBlock: false,
            captchaId: 0,
            key: null,
            model: {},
            step1Form: {
                code: null,
                result: true,
                isEmailTaken: false,
                isCodeExpired: false,
                resendingCode: false,
                resendCount: 0,
                captchaResponse : null
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
            register: register,
            createCaptcha: createCaptcha,
            successCaptcha: successCaptcha,
            errorCaptcha: errorCaptcha
        };

        vm.init = function(key, resendCount) {
            vm.data.key = key;
            vm.data.step1Form.resendCount = resendCount;
        };

        function verifyEmail() {
            vm.data.loading = true;
            registerService.verifyEmail(vm.data.key, vm.data.step1Form.code).then(function (result) {
                if (result.code !== null) {
                    vm.data.model.returnUrl = result.code.returnUrl;
                    vm.data.model.referer = result.code.referer;
                    vm.data.model.email = result.code.email;
                    vm.data.model.cid = result.code.cid;
                    vm.data.model.traffic = result.code.traffic;
                    vm.data.step1Form.isEmailTaken = result.isEmailTaken;
                    vm.data.step1Form.result = true;

                    if (!result.isEmailTaken) {
                        vm.data.step = 2;
                    }
                } else {
                    vm.data.step1Form.result = false;
                    vm.data.step1Form.isCodeExpired = result.isCodeExpired;
                }

                vm.data.loading = false;
            });
        }

        function resendCode() {
            if (vm.data.step1Form.resendingCode || vm.data.step1Form.resendCount > 2)
                return;

            vm.data.step1Form.resendingCode = true;
            registerService.resendCode(vm.data.key, vm.data.step1Form.captchaResponse).then(function (result) {
                vm.data.step1Form.isCodeExpired = result.isCodeExpired;
                
                if (result.result) {
                    $.notify({ title: 'Code successfully sent!' }, { className: 'success' });
                    vm.data.step1Form.resendCount++;
                    vm.data.step1Form.captchaResponse = null;
                    vm.data.showResendBlock = false;
                } 

                if (!result.isCodeExpired) {
                    vcRecaptchaService.reload(vm.data.captchaId);
                }
                
                vm.data.step1Form.resendingCode = false;
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
            vm.data.model.key = vm.data.key;
            vm.data.loading = true;
            registerService.register(vm.data.model).then(function (result) {
                vm.data.loading = false;
                
                if (result.errors.length) {
                    vm.data.summaryErrors = result.errors;
                }
                else {
                    if (!result.isPasswordComplex) {
                        vm.data.step = 2;
                    } else{
                        window.location = vm.data.model.returnUrl ? vm.data.model.returnUrl : '/';
                    }
                }
            });
        }
        
        function createCaptcha(id){
            vm.data.captchaId = id;
        }

        function successCaptcha(token){
            vm.data.step1Form.captchaResponse = token;
        }

        function errorCaptcha(){
            vm.data.step1Form.captchaResponse = null;
        }

        function goToLogin() {
            window.location = '/signin';
        }
    }
})();
