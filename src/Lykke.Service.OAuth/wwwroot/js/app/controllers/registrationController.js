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
            ukUserQuestionnaire: {
                investorType: null,
            },
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
            ukUserQuestionnaireInvestorTypeForm: {
                selectedAnswerIndex: 0
            },
            ukUserQuestionnaireSophisticatedInvestorStatementForm: {
                selectedAnswerIndex: null,
                authorisedFirmName: null,
                date: null,
                isSigned: false,
                question: "Please confirm whether you qualify as a sophisticated investor on the basis that in the last three years you have received a certificate from an authorised firm confirming you understand the risks involved with high - risk investments.",
                firmNameQuestion: "If yes, what is the name of the authorised firm?",
                isFilled: () => {
                    return !(vm.data.ukUserQuestionnaireSophisticatedInvestorStatementForm.selectedAnswerIndex === null ||
                        !vm.data.ukUserQuestionnaireSophisticatedInvestorStatementForm.isSigned ||
                        (
                            vm.data.ukUserQuestionnaireSophisticatedInvestorStatementForm.selectedAnswerIndex == 1 &&
                            vm.data.ukUserQuestionnaireSophisticatedInvestorStatementForm.authorisedFirmName == null
                        ));
                }
            },
            ukUserQuestionnaireForm: {
                currentQuestionnaireEntry: null,
                currentQuestionIndex: 0,
                selectedAnswerIndex: null,
                answers: {},
                answerIndexes: {}
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
            beginUkQuestionnaire: beginUkQuestionnaire,
            ukQuestionnaireInvestorTypeBack: ukQuestionnaireInvestorTypeBack,
            setUkQuestionnaireInvestorTypeAnswer: setUkQuestionnaireInvestorTypeAnswer,
            setUkQuestionnaireSophisticatedInvestorStatementAnswer: setUkQuestionnaireSophisticatedInvestorStatementAnswer,
            ukQuestionnaireInvestorStatementBack: ukQuestionnaireInvestorStatementBack,
            setUkQuestionnaireAnswer: setUkQuestionnaireAnswer,
            ukQuestionnaireBack: ukQuestionnaireBack,
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

        const ukInvestorType = {
            CertifiedSophisticated: "CertifiedSophisticated",
            SelfCertifiedSophisticated: "SelfCertifiedSophisticated",
            Restricted: "Restricted",
            HighNetWorthIndividual: "HighNetWorthIndividual"
        };

        vm.ukUserQuestionnaire = {
            "investorTypeClassificationAnswers": [
                {
                    title: "Certified Sophisticated investor",
                    description: "In the last three years you have received a certificate from an authorised firm confirming you understand the risks involved with high-risk investments",
                    investorType: ukInvestorType.CertifiedSophisticated
                },
                {
                    title: "Self-Certified Sophisticated investor",
                    description: "You have worked in private equity, or been a director of a company with turnover of over GBP 1 million, or are a member of network of business angels for over 6 months or have made 2 or more investments in unlisted companies",
                    investorType: ukInvestorType.SelfCertifiedSophisticated
                },
                {
                    title: "Restricted Investor",
                    description: "You have not invested more than 10% of your net assets in high-risk investments in the past 12 months or intend to do so in the next 12 months",
                    investorType: ukInvestorType.Restricted
                },
                {
                    title: "High net worth individual",
                    description: "You have an annual income of GBP 100,000 and/or net assets over GBP 250,000",
                    investorType: ukInvestorType.HighNetWorthIndividual
                }
            ],
            "generalQuestions": [
                {
                    question: "What does Lykke charge for holding customer’s cryptoassets in custody?",
                    answers: [
                        "Lykke charges a monthly fee.",
                        "Lykke does not charge anything for holding cryptoassets in custody.",
                        "Lykke charges a variable fee based on the value of the customers’ assets."
                    ]
                },
                {
                    question: "What occurs with your cryptocurrency holdings in the event of Lykke's insolvency?",
                    answers: [
                        "My cryptocurrency holdings are safeguarded against Lykke's insolvency. Nevertheless, there could be a potential delay in retrieving them, potentially hindering my ability to sell if market prices were declining.",
                        "My cryptoassets are immediately exchanged to fiat currencies (GBP, EUR, USD, CHF), which I can withdraw to my bank account.",
                        "I can continue to trade and access my cryptoassets even if Lykke becomes insolvent."
                    ]
                },
                {
                    question: "How frequently does the value of cryptoassets change?",
                    answers: [
                        "The value fluctuates rarely as it solely relies on the stability and structure of the underlying protocol/blockchain.",
                        "The value can change frequently because cryptoassets are generally traded 24/7/365 in a global market.",
                        "Cryptoassets can only be traded during normal UK market hours and their value will not change outside these times."
                    ]
                },
                {
                    question: "Which one of the following statements is CORRECT?",
                    answers: [
                        "Cryptoassets are only traded on regulated exchanges.",
                        "Market data concerning cryptoassets maintains reliability as it consistently undergoes publication via regulated market data providers.",
                        "None of the above statements are correct."
                    ]
                },
                {
                    question: "What is the most sensible strategy for investing in high-risk investments like cryptoassets for retail investors?",
                    answers: [
                        "To invest everything into a single cryptoasset and hold it for at least 5 years.",
                        "To only invest in cryptoassets as they generate the highest possible returns.",
                        "To not invest more than 10% of net assets in cryptoassets."
                    ]
                },
                {
                    question: "What is a good way to learn about the risks and rewards of investing in cryptoassets?",
                    answers: [
                        "Start by investing in Bitcoin first as it’s the lowest risk cryptoasset.",
                        "Social media influencers offer valuable insights regarding which cryptoassets might yield the most substantial returns.",
                        "Use online learning resources and the Lykke blog to learn about investing in cryptoassets and the associated risks."
                    ]
                },
                {
                    question: "What regulatory protections currently apply to cryptoassets in the UK?",
                    answers: [
                        "They are protected under the UK Financial Services Compensation Scheme like any other investment.",
                        "The UK Financial Ombudsman Service will handle any complaint relating to my cryptoasset investments.",
                        "There are no regulatory protections for cryptoassets."
                    ]
                },
                {
                    question: "What are the main differences between the risks of cryptoassets and regular currencies (GBP, EUR, USD, etc.)?",
                    answers: [
                        "Cryptocurrencies lack government backing akin to traditional currencies (GBP, USD, EUR, etc.), and there is no central bank to implement measures safeguarding their value during a crisis.",
                        "There are no significant differences and traders on all markets are equally protected from risks.",
                        "Cryptoassets have intrinsic value while fiat currencies (GBP, EUR, USD, etc.) do not."
                    ]
                },
                {
                    question: "What is the risk if the Lykke exchange is not available because there is an operational outage?",
                    answers: [
                        "I will be unable to sell my cryptocurrency, and a potential decrease in the market price of the asset may lead to diminishing the value of my investment.",
                        "There is no risk because the price will remain the same once the exchange is available again.",
                        "There is no risk because Lykke will compensate me for any losses due to an operational outage."
                    ]
                },
                {
                    question: "Can you always sell your cryptoassets?",
                    answers: [
                        "No, in the case of low liquidity for cryptoassets, there might not be a buyer available at the specific time and price at which I intend to sell.",
                        "Yes, there’s always a buyer for any cryptoasset.",
                        "Yes, cryptoasset liquidity is always available."
                    ]
                }
            ]
        };

        vm.init = function(key, affiliateCode, resendCount) {
            vm.data.key = key;
            vm.data.step1Form.resendCount = resendCount;
            vm.data.step5Form.affiliateCode = affiliateCode;
            vm.data.step1Form.countriesTask = getCountries();

            vm.data.ukUserQuestionnaireForm.currentQuestionnaireEntry = vm.ukUserQuestionnaire.generalQuestions[0];
            vm.data.ukUserQuestionnaireForm.currentQuestionIndex = 0;
            vm.data.ukUserQuestionnaireForm.selectedAnswerIndex = null;
            vm.data.ukUserQuestionnaireForm.answers = {};
            vm.data.ukUserQuestionnaireForm.answerIndexes = {};

            vm.data.ukUserQuestionnaireInvestorTypeForm.selectedAnswerIndex = null;

            vm.data.ukUserQuestionnaireSophisticatedInvestorStatementForm.selectedAnswerIndex = null;
            vm.data.ukUserQuestionnaireSophisticatedInvestorStatementForm.authorisedFirmName = null;
            vm.data.ukUserQuestionnaireSophisticatedInvestorStatementForm.date = new Date();
            vm.data.ukUserQuestionnaireSophisticatedInvestorStatementForm.isSigned = false;
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
                        vm.data.step1Form.countriesTask.then(function () {
                            vm.data.loading = false;
                            setStep(2);
                        },
                        function(){
                            getCountries().then(function () {
                                vm.data.loading = false;
                                setStep(2);
                            }, function () {
                                technicalProblems();
                            });
                        });
                    }
                } else {
                    vm.data.loading = false;
                    vm.data.step1Form.result = false;
                    vm.data.step1Form.isCodeExpired = result.isCodeExpired;
                }
            }, function () {
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
            }, function () {
                vm.data.step1Form.resendingCode = false;
                technicalProblems("Technical problem");
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
            }, function () {
                technicalProblems("Technical problems during phone number verification");
            });
        }

        function setPhoneCode() {
            vm.data.loading = true;
            vm.data.model.code = vm.data.step3Form.code;
            registerService.verifyPhone(
                vm.data.key,
                vm.data.model.code,
                vm.data.model.phone,
                vm.data.model.countryOfResidence).then(function (result) {

                    vm.data.loading = false;

                    if (result.isValid) {
                        if (result.isUkUser) {
                            setStep('ukQuestionnaireHelp');
                        } else {
                            setStep(4);
                        }
                    }
                    else {
                        vm.data.step3Form.result = false;
                        vm.data.step3Form.isNotValidCode = !result.isValid;
                    }
                }, function () {
                    technicalProblems();
                });
        }

        function beginUkQuestionnaire() {

            if (!vm.data.model.ukUserQuestionnaire) {
                vm.data.model.ukUserQuestionnaire = {};
            }

            setStep('ukQuestionnaireInvestorType');
        }

        function ukQuestionnaireInvestorTypeBack() {
            setStep('ukQuestionnaireHelp');
        }

        function setUkQuestionnaireInvestorTypeAnswer() {

            let answerIndex = vm.data.ukUserQuestionnaireInvestorTypeForm.selectedAnswerIndex;
            let answer = vm.ukUserQuestionnaire.investorTypeClassificationAnswers[answerIndex];

            vm.data.ukUserQuestionnaire.investorType = answer.investorType;
            vm.data.model.ukUserQuestionnaire.investorTypeAnswer = answer;

            setStep('ukQuestionnaireInvestorStatement');
        }

        function setUkQuestionnaireSophisticatedInvestorStatementAnswer() {

            let question = vm.data.ukUserQuestionnaireSophisticatedInvestorStatementForm.question;
            let firmNameQuestion = vm.data.ukUserQuestionnaireSophisticatedInvestorStatementForm.firmNameQuestion;
            let answerIndex = vm.data.ukUserQuestionnaireSophisticatedInvestorStatementForm.selectedAnswerIndex;
            let firmName = vm.data.ukUserQuestionnaireSophisticatedInvestorStatementForm.authorisedFirmName;
            let isSigned = vm.data.ukUserQuestionnaireSophisticatedInvestorStatementForm.isSigned;
            let date = vm.data.ukUserQuestionnaireSophisticatedInvestorStatementForm.date;
            let statement = {
                isSigned: isSigned,
                date: date
            };

            switch (answerIndex) {
                case 0:
                    statement[question] = "No";
                    break;

                case 1:
                    statement[question] = "Yes";
                    statement[firmNameQuestion] = firmName;
                    break;

                case 2:
                    statement[question] = "This does not apply to me.";
                    break;

                default:
                    throw "Unknown answer: " + answerIndex;
            }

            vm.data.model.ukUserQuestionnaire.investorStatement = statement;

            setStep('ukQuestionnaire');
        }

        function ukQuestionnaireInvestorStatementBack() {
            setStep('ukQuestionnaireInvestorType');
        }

        function setUkQuestionnaireAnswer() {

            let questionIndex = vm.data.ukUserQuestionnaireForm.currentQuestionIndex;
            let nextQuestionIndex = questionIndex + 1;
            let questionnaireEntry = vm.ukUserQuestionnaire.generalQuestions[questionIndex];
            let question = questionnaireEntry.question;
            let answerIndex = vm.data.ukUserQuestionnaireForm.selectedAnswerIndex;
            let answer = questionnaireEntry.answers[answerIndex];

            vm.data.ukUserQuestionnaireForm.answers[question] = answer;
            vm.data.ukUserQuestionnaireForm.answerIndexes[questionIndex] = answerIndex;
            vm.data.ukUserQuestionnaireForm.isLastQuestion = nextQuestionIndex == vm.ukUserQuestionnaire.generalQuestions.length - 1;

            if (nextQuestionIndex < vm.ukUserQuestionnaire.generalQuestions.length) {
                vm.data.ukUserQuestionnaireForm.currentQuestionIndex = nextQuestionIndex;
                vm.data.ukUserQuestionnaireForm.currentQuestionnaireEntry = vm.ukUserQuestionnaire.generalQuestions[nextQuestionIndex];
                if (vm.data.ukUserQuestionnaireForm.answerIndexes[nextQuestionIndex] === undefined) {
                    vm.data.ukUserQuestionnaireForm.selectedAnswerIndex = null;
                } else {
                    vm.data.ukUserQuestionnaireForm.selectedAnswerIndex = vm.data.ukUserQuestionnaireForm.answerIndexes[nextQuestionIndex];
                }            
            } else {
                vm.data.model.ukUserQuestionnaire.generalAnswers = vm.data.ukUserQuestionnaireForm.answers;

                setStep(4);
            }
        }        

        function ukQuestionnaireBack() {

            if (vm.data.ukUserQuestionnaireForm.currentQuestionIndex == 0) {
                setStep('ukQuestionnaireInvestorStatement');
                return;
            }

            let questionIndex = vm.data.ukUserQuestionnaireForm.currentQuestionIndex;
            let nextQuestionIndex = questionIndex - 1;

            vm.data.ukUserQuestionnaireForm.currentQuestionIndex = nextQuestionIndex;
            vm.data.ukUserQuestionnaireForm.isLastQuestion = false;

            vm.data.ukUserQuestionnaireForm.currentQuestionnaireEntry = vm.ukUserQuestionnaire.generalQuestions[nextQuestionIndex];

            if (vm.data.ukUserQuestionnaireForm.answerIndexes[nextQuestionIndex] === undefined) {
                vm.data.ukUserQuestionnaireForm.selectedAnswerIndex = null;
            } else {
                vm.data.ukUserQuestionnaireForm.selectedAnswerIndex = vm.data.ukUserQuestionnaireForm.answerIndexes[nextQuestionIndex];
            }            
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

            vm.data.step5Form.affCodeTask.then(function () {
                if (!vm.data.step5Form.affiliateCodeCorrect) {
                    vm.data.loading = false;
                    return;
                }

                registerService.register(vm.data.model).then(function (result) {
                    if (!hasErrors(result)) {
                        window.location = vm.data.model.returnUrl ? vm.data.model.returnUrl : '/';
                    } else {
                        vm.data.loading = false;

                        if (result.errors.length) {
                            vm.data.summaryErrors = result.errors;
                            return;
                        }

                        if (!result.isPasswordComplex) {
                            setStep(4);
                            return;
                        }

                        if (!result.isAffiliateCodeCorrect) {
                            vm.data.step5Form.affiliateCodeCorrect = false;
                            return;
                        }

                        if (result.registrationResponse && result.registrationResponse.account && result.registrationResponse.account.state !== 0) {
                            vm.data.step5Form.state = result.registrationResponse.account.state;
                            vm.data.step5Form.hideForm = true;
                        }
                    }
                }, function () {
                    technicalProblems();
                }).catch(function () {
                    technicalProblems();
                });
            }, function () {
                technicalProblems();
            }).catch(function () {
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

        function getCountries() {
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

        function setStep(step) {
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
