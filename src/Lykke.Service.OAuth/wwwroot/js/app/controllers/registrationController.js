(function () {
    'use strict';

    angular.module('registerApp').controller('registrationController', registrationController);

    registrationController.$inject = ['registerService', 'vcRecaptchaService'];

    function registrationController(registerService, vcRecaptchaService) {
        var vm = this;

        vm.data = {
            uimask: " (999) 999-9999",
            defaultMask: " (999) 999-9999",
            step: 1,
            loading: false,
            showResendBlock: false,
            captchaId: 0,
            key: null,
            model: {},
            countries: [],
            isAutoSelect: true,
            selectedCountry: null,
            selectedCountryName: null,
            selectedPrefix: null,
            step1Form: {
                code: null,
                result: true,
                isEmailTaken: false,
                isCodeExpired: false,
                resendingCode: false,
                resendCount: 0,
                captchaResponse: null
            },
            step2Form: {
                phone: null
            },
            step3Form: {
                code: null,
                isNotValidCode: false,
            },
            step4Form: {
                phone: null,
                code: null,
                showPassword: false,
                showConfirmPassword: false,
                hint: null
            },
            step5Form: {},
            summaryErrors: []
        };

        vm.handlers = {
            goToLogin: goToLogin,
            verifyEmail: verifyEmail,
            setPhoneCode: setPhoneCode,
            resendCode: resendCode,
            isStep4FormSubmitDisabled: isStep4FormSubmitDisabled,
            setPassword: setPassword,
            setPhone: setPhone,
            register: register,
            createCaptcha: createCaptcha,
            successCaptcha: successCaptcha,
            errorCaptcha: errorCaptcha,
            changeCountry: changeCountry,
            confirmPhone: confirmPhone
        };

        vm.init = function (key, resendCount) {
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
                        registerService.getCountries().then(function (result) {
                            vm.data.countries = result.data;
                            var result = vm.data.countries.filter(obj => {
                                return obj.selected == true
                            });
                            if (result.length !== 0) {
                                vm.data.uimask = result[0].prefix + vm.data.defaultMask;
                                vm.data.selectedPrefix = result[0].prefix;
                                vm.data.selectedCountryName = result[0].title;
                            }
                            vm.data.step = 2;
                        });

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

        function changeCountry() {
            var result = vm.data.countries.filter(obj => {
                return obj.id === vm.data.selectedCountry
            });
            vm.data.uimask = result[0].prefix + vm.data.defaultMask;
            vm.data.selectedPrefix = result[0].prefix;
            vm.data.selectedCountryName = result[0].title;
            vm.data.isAutoSelect = false;
        }
        function confirmPhone() {
            if (vm.data.selectedPrefix === null)
                return;
            if (vm.data.isAutoSelect)
                $("#modal_message").modal('show');
            else setPhone();
        }

        function setPhone() {
            vm.data.loading = true;
            vm.data.model.phone = vm.data.selectedPrefix + vm.data.step2Form.phone;
            registerService.sendPhoneCode(vm.data.key, vm.data.model.phone).then(function (result) {
                vm.data.step2Form.result = true;
                vm.data.loading = false;
                vm.data.step = 3;
            });
        }

        function setPhoneCode() {
            vm.data.loading = true;
            vm.data.model.code = vm.data.step3Form.code;
            registerService.verifyPhone(vm.data.key, vm.data.model.code, vm.data.model.phone).then(function (result) {
                if (result.isValid) {
                    vm.data.loading = false;
                    vm.data.step = 4;
                }
                else {
                    vm.data.step3Form.result = false;
                    vm.data.step3Form.isNotValidCode = !result.isValid;
                    vm.data.loading = false;
                }
            });
        }

        function isStep4FormSubmitDisabled() {
            return !vm.step4Form.$valid ||
                (!vm.data.step4Form.password.length || !vm.data.step4Form.confirmPassword.length) ||
                vm.data.step4Form.password !== vm.data.step4Form.confirmPassword;
        }
        function setPassword() {
            vm.data.model.password = vm.data.step4Form.password;
            vm.data.model.hint = vm.data.step4Form.hint;
            vm.data.step = 5;
        }

        function register() {
            vm.data.model.firstName = vm.data.step5Form.firstName;
            vm.data.model.lastName = vm.data.step5Form.lastName;
            vm.data.model.key = vm.data.key;
            vm.data.loading = true;
            registerService.register(vm.data.model).then(function (result) {
                if (result.errors.length) {
                    vm.data.summaryErrors = result.errors;
                    vm.data.loading = false;
                }
                else {
                    if (!result.isPasswordComplex) {
                        vm.data.step = 4;
                        vm.data.loading = false;
                    } else {
                        window.location = vm.data.model.returnUrl ? vm.data.model.returnUrl : '/';
                    }
                }
            });
        }

        function createCaptcha(id) {
            vm.data.captchaId = id;
        }

        function successCaptcha(token) {
            vm.data.step1Form.captchaResponse = token;
        }

        function errorCaptcha() {
            vm.data.step1Form.captchaResponse = null;
        }

        function goToLogin() {
            window.location = '/signin';
        }
    }
})();
