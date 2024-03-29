﻿(function () {
    'use strict';

    angular.module('registerApp').controller('registrationController', registrationController);

    registrationController.$inject = ['registerService', 'vcRecaptchaService', '$q'];

    function registrationController(registerService, vcRecaptchaService, $q) {
        var vm = this;

        vm.data = {
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
                captchaResponse : null,
                countriesTask: null
            },
            step2Form: {
                phone: null,
                countryOfResidence: null,
                isPhoneTaken: false
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
            step5Form: {
                state: 0,
                hideForm: false,
                firstName: null,
                lastName: null,
                affiliateCode: null,
                affiliateCodeCorrect: true,
                affCodeTask: null
            },
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
            changeCountryOfResidence: changeCountryOfResidence,
            confirmPhone: confirmPhone,
            checkAffiliateCode: checkAffiliateCode
        };

        vm.init = function(key, affiliateCode, resendCount) {
            vm.data.key = key;
            vm.data.step1Form.resendCount = resendCount;
            vm.data.step5Form.affiliateCode = affiliateCode;
            vm.data.step1Form.countriesTask = getCountries();
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
                        vm.data.step1Form.countriesTask.then(function(){
                            setStep(2);
                        },
                        function(){
                            getCountries().then(function(){
                                setStep(2);
                                },
                                function () {
                                    technicalProblems();
                                })
                        });
                    }
                } else {
                    vm.data.step1Form.result = false;
                    vm.data.step1Form.isCodeExpired = result.isCodeExpired;
                    vm.data.loading = false;
                }
            }, function(){
                technicalProblems("Technical problems during email verification");
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
                return obj.id === vm.data.selectedCountry;
            });
            vm.data.step2Form.phone = result[0].prefix;
            vm.data.selectedCountryName = result[0].title;
        }

        function changeCountryOfResidence() {
            vm.data.isAutoSelect = false;
        }

        function confirmPhone() {
            if (!vm.data.step2Form.phone)
                return;
            if (!vm.data.step2Form.countryOfResidence)
                return;
            if (vm.data.isAutoSelect)
                $("#modal_message").modal('show');
            else setPhone();
        }

        function setPhone() {
            vm.data.loading = true;
            vm.data.model.phone = vm.data.step2Form.phone;
            vm.data.model.countryOfResidence = vm.data.step2Form.countryOfResidence;
            registerService.sendPhoneCode(vm.data.key, vm.data.model.phone).then(function (result) {
                vm.data.loading = false;
                vm.data.step2Form.result = true;
                vm.data.step2Form.isPhoneTaken = result.isPhoneTaken;
                if(!result.isPhoneTaken) {
                    setStep(3);
                }
            }, function(){
                technicalProblems("Technical problems during phone number verification");
            });
        }

        function setPhoneCode() {
            vm.data.loading = true;
            vm.data.model.code = vm.data.step3Form.code;
            registerService.verifyPhone(vm.data.key, vm.data.model.code, vm.data.model.phone).then(function (result) {
                if (result.isValid) {
                    vm.data.loading = false;
                    setStep(4);
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
            setStep(5);
        }

        function register() {
            vm.data.model.firstName = vm.data.step5Form.firstName;
            vm.data.model.lastName = vm.data.step5Form.lastName;
            vm.data.model.affiliateCode = vm.data.step5Form.affiliateCode;
            vm.data.model.key = vm.data.key;
            vm.data.loading = true;

            if (!vm.data.step5Form.affCodeTask){
                checkAffiliateCode();
            }

            vm.data.step5Form.affCodeTask.then(function(){
                if (!vm.data.step5Form.affiliateCodeCorrect){
                    vm.data.loading = false;
                    return;
                }

                registerService.register(vm.data.model).then(function (result) {
                    if (!hasErrors(result)){
                        window.location = vm.data.model.returnUrl ? vm.data.model.returnUrl : '/';
                    } else{
                        vm.data.loading = false;

                        if (result.errors.length) {
                            vm.data.summaryErrors = result.errors;
                            return;
                        }

                        if (!result.isPasswordComplex) {
                            setStep(4);
                            return;
                        }

                        if (!result.isAffiliateCodeCorrect){
                            vm.data.step5Form.affiliateCodeCorrect = false;
                            return;
                        }

                        if (result.registrationResponse && result.registrationResponse.account && result.registrationResponse.account.state !== 0) {
                            vm.data.step5Form.state = result.registrationResponse.account.state;
                            vm.data.step5Form.hideForm = true;
                        }
                    }
                }, function(){
                    technicalProblems();
                })
                .catch(function() {
                    technicalProblems();
                });
            }, function(){
                technicalProblems();
            })
            .catch(function(){
                technicalProblems();
            });
        }

        function hasErrors(result){
            return result.errors.length ||
                !result.isValid ||
                result.registrationResponse && result.registrationResponse.account && result.registrationResponse.account.state !== 0;
        }

        function technicalProblems(message){
            message = message || "Technical problems during registration";
            vm.data.summaryErrors.push(message);
            vm.data.loading = false;
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

        function getCountries(){
            return registerService.getCountries().then(function (result) {
                vm.data.countries = result.data;
                var selected = vm.data.countries.filter(obj => {
                    return obj.selected === true;
                });

                if (selected.length !== 0) {
                    vm.data.step2Form.phone = selected[0].prefix;
                    vm.data.selectedPrefix = selected[0].prefix;
                    vm.data.selectedCountryName = selected[0].title;
                    vm.data.step2Form.countryOfResidence = selected[0].id;
                }
            });
        }

        function setStep(step){
            vm.data.summaryErrors = [];
            vm.data.step = step;
        }

        function checkAffiliateCode(isBlur){
            if (isBlur === false) {
                if (!vm.data.step5Form.affiliateCode){
                    vm.data.step5Form.affiliateCodeCorrect = true
                }

                return;
            }

            if (vm.data.step5Form.affiliateCode){
                vm.data.step5Form.affCodeTask = registerService.checkAffiliateCode(vm.data.step5Form.affiliateCode);
                vm.data.step5Form.affCodeTask.then(function(result){
                    vm.data.step5Form.affiliateCodeCorrect = result;
                });
            } else{
                vm.data.step5Form.affCodeTask = $q.resolve();
                vm.data.step5Form.affiliateCodeCorrect = true;
            }
        }
    }
})();
