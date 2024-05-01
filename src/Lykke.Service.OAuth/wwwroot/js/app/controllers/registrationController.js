(function () {
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
            ukUserQuestionnaireSISForm: {
                selectedAnswerIndex: null,
                yesExtraAnswer: null,
                date: null,
                isSigned: false,
                isFilled: () => {
                    return vm.data.ukUserQuestionnaireSISForm.isSigned &&
                        vm.data.ukUserQuestionnaireSISForm.selectedAnswerIndex == 1 &&
                        vm.data.ukUserQuestionnaireSISForm.yesExtraAnswer != null &&
                        vm.data.ukUserQuestionnaireSISForm.yesExtraAnswer.length > 0;
                }
            },
            ukUserQuestionnaireSCSISForm: {
                aSelectedAnswerIndex: null,
                bSelectedAnswerIndex: null,
                cSelectedAnswerIndex: null,
                dSelectedAnswerIndex: null,
                aYesExtraAnswer: null,
                bYesExtraAnswer: null,
                cYesExtraAnswer: null,
                dYesExtraAnswer: null,
                date: null,
                isSigned: false,
                isFilled: () => {
                    return vm.data.ukUserQuestionnaireSCSISForm.isSigned &&
                        (
                            vm.data.ukUserQuestionnaireSCSISForm.aSelectedAnswerIndex == 1 &&
                            vm.data.ukUserQuestionnaireSCSISForm.aYesExtraAnswer != null &&
                            vm.data.ukUserQuestionnaireSCSISForm.aYesExtraAnswer.length > 0 ||

                            vm.data.ukUserQuestionnaireSCSISForm.bSelectedAnswerIndex == 1 &&
                            vm.data.ukUserQuestionnaireSCSISForm.bYesExtraAnswer != null &&
                            vm.data.ukUserQuestionnaireSCSISForm.bYesExtraAnswer.length > 0 ||

                            vm.data.ukUserQuestionnaireSCSISForm.cSelectedAnswerIndex == 1 &&
                            vm.data.ukUserQuestionnaireSCSISForm.cYesExtraAnswer != null &&
                            vm.data.ukUserQuestionnaireSCSISForm.cYesExtraAnswer.length > 0 ||

                            vm.data.ukUserQuestionnaireSCSISForm.dSelectedAnswerIndex == 1 &&
                            vm.data.ukUserQuestionnaireSCSISForm.dYesExtraAnswer != null &&
                            vm.data.ukUserQuestionnaireSCSISForm.dYesExtraAnswer.length > 0
                        );
                }
            },
            ukUserQuestionnaireRISForm: {
                aSelectedAnswerIndex: null,
                bSelectedAnswerIndex: null,
                aYesExtraAnswer: null,
                bYesExtraAnswer: null,
                date: null,
                isSigned: false,
                isFilled: () => {
                    return vm.data.ukUserQuestionnaireRISForm.isSigned &&
                        vm.data.ukUserQuestionnaireRISForm.aSelectedAnswerIndex == 1 &&
                        vm.data.ukUserQuestionnaireRISForm.aYesExtraAnswer != null &&
                        vm.data.ukUserQuestionnaireRISForm.aYesExtraAnswer.length > 0 &&

                        vm.data.ukUserQuestionnaireRISForm.bSelectedAnswerIndex == 1 &&
                        vm.data.ukUserQuestionnaireRISForm.bYesExtraAnswer != null &&
                        vm.data.ukUserQuestionnaireRISForm.bYesExtraAnswer.length > 0;
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
            setUkQuestionnaireSISAnswer: setUkQuestionnaireSISAnswer,
            setUkQuestionnaireSCSISAnswer: setUkQuestionnaireSCSISAnswer,
            setUkQuestionnaireRISAnswer: setUkQuestionnaireRISAnswer,
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

            vm.data.ukUserQuestionnaireSISForm.selectedAnswerIndex = null;
            vm.data.ukUserQuestionnaireSISForm.yesExtraAnswer = null;
            vm.data.ukUserQuestionnaireSISForm.date = new Date();
            vm.data.ukUserQuestionnaireSISForm.isSigned = false;

            vm.data.ukUserQuestionnaireSCSISForm.aSelectedAnswerIndex = null;
            vm.data.ukUserQuestionnaireSCSISForm.aYesExtraAnswer = null;
            vm.data.ukUserQuestionnaireSCSISForm.bSelectedAnswerIndex = null;
            vm.data.ukUserQuestionnaireSCSISForm.bYesExtraAnswer = null;
            vm.data.ukUserQuestionnaireSCSISForm.cSelectedAnswerIndex = null;
            vm.data.ukUserQuestionnaireSCSISForm.cYesExtraAnswer = null;
            vm.data.ukUserQuestionnaireSCSISForm.dSelectedAnswerIndex = null;
            vm.data.ukUserQuestionnaireSCSISForm.dYesExtraAnswer = null;
            vm.data.ukUserQuestionnaireSCSISForm.date = new Date();
            vm.data.ukUserQuestionnaireSCSISForm.isSigned = false;

            vm.data.ukUserQuestionnaireRISForm.aSelectedAnswerIndex = null;
            vm.data.ukUserQuestionnaireRISForm.aYesExtraAnswer = null;
            vm.data.ukUserQuestionnaireRISForm.bSelectedAnswerIndex = null;
            vm.data.ukUserQuestionnaireRISForm.bYesExtraAnswer = null;
            vm.data.ukUserQuestionnaireRISForm.date = new Date();
            vm.data.ukUserQuestionnaireRISForm.isSigned = false;
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

        function setUkQuestionnaireSISAnswer() {

            var question = angular.element('#ukUserQuestionnaireSISForm_Question').html();
            var yesExtraQuestion = angular.element("#ukUserQuestionnaireSISForm_YesExtraQuestion").html();
            let answerIndex = vm.data.ukUserQuestionnaireSISForm.selectedAnswerIndex;
            let yesExtraAnswer = vm.data.ukUserQuestionnaireSISForm.yesExtraAnswer;
            let isSigned = vm.data.ukUserQuestionnaireSISForm.isSigned;
            let date = vm.data.ukUserQuestionnaireSISForm.date;
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
                    statement[yesExtraQuestion] = yesExtraAnswer;
                    break;

                default:
                    throw "Unknown answer: " + answerIndex;
            }

            vm.data.model.ukUserQuestionnaire.investorStatement = statement;

            setStep('ukQuestionnaire');
        }

        function setUkQuestionnaireSCSISAnswer() {

            var generalQuestion = angular.element('#ukUserQuestionnaireSCSISForm_GeneralQuestion').html();
            var questions = [
                angular.element("#ukUserQuestionnaireSCSISForm_AQuestion").html(),
                angular.element("#ukUserQuestionnaireSCSISForm_BQuestion").html(),
                angular.element("#ukUserQuestionnaireSCSISForm_CQuestion").html(),
                angular.element("#ukUserQuestionnaireSCSISForm_DQuestion").html()];
            var yesExtraQuestions = [
                angular.element("#ukUserQuestionnaireSCSISForm_AYesExtraQuestion").html(),
                angular.element("#ukUserQuestionnaireSCSISForm_BYesExtraQuestion").html(),
                angular.element("#ukUserQuestionnaireSCSISForm_CYesExtraQuestion").html(),
                angular.element("#ukUserQuestionnaireSCSISForm_DYesExtraQuestion").html()];
            let answerIndexes = [
                vm.data.ukUserQuestionnaireSCSISForm.aSelectedAnswerIndex,
                vm.data.ukUserQuestionnaireSCSISForm.bSelectedAnswerIndex,
                vm.data.ukUserQuestionnaireSCSISForm.cSelectedAnswerIndex,
                vm.data.ukUserQuestionnaireSCSISForm.dSelectedAnswerIndex];
            let yesExtraAnswers = [
                vm.data.ukUserQuestionnaireSCSISForm.aYesExtraAnswer,
                vm.data.ukUserQuestionnaireSCSISForm.bYesExtraAnswer,
                vm.data.ukUserQuestionnaireSCSISForm.cYesExtraAnswer,
                vm.data.ukUserQuestionnaireSCSISForm.dYesExtraAnswer];
            let isSigned = vm.data.ukUserQuestionnaireSCSISForm.isSigned;
            let date = vm.data.ukUserQuestionnaireSCSISForm.date;
            let statement = {
                generalQuestion: generalQuestion,
                isSigned: isSigned,
                date: date
            };

            for (var i = 0; i < questions.length; ++i) {

                let question = questions[i];
                let answerIndex = answerIndexes[i];
                let yesExtraQuestion = yesExtraQuestions[i];
                let yesExtraAnswer = yesExtraAnswers[i];

                switch (answerIndex) {
                    case 0:
                        statement[question] = "No";
                        break;

                    case 1:
                        statement[question] = "Yes";
                        statement[yesExtraQuestion] = yesExtraAnswer;
                        break;

                    default:
                        throw "Unknown answer: " + answerIndex + " question index:" + i;
                }
            }

            vm.data.model.ukUserQuestionnaire.investorStatement = statement;

            setStep('ukQuestionnaire');
        }

        function setUkQuestionnaireRISAnswer() {

            var generalQuestion = angular.element('#ukUserQuestionnaireRISForm_GeneralQuestion').html();
            var questions = [
                angular.element("#ukUserQuestionnaireRISForm_AQuestion").html(),
                angular.element("#ukUserQuestionnaireRISForm_BQuestion").html()];
            var yesExtraQuestions = [
                angular.element("#ukUserQuestionnaireRISForm_AYesExtraQuestion").html(),
                angular.element("#ukUserQuestionnaireRISForm_BYesExtraQuestion").html()];
            let answerIndexes = [
                vm.data.ukUserQuestionnaireRISForm.aSelectedAnswerIndex,
                vm.data.ukUserQuestionnaireRISForm.bSelectedAnswerIndex];
            let yesExtraAnswers = [
                vm.data.ukUserQuestionnaireRISForm.aYesExtraAnswer,
                vm.data.ukUserQuestionnaireRISForm.bYesExtraAnswer];
            let noLabels = [
                angular.element("#ukUserQuestionnaireRISForm_ANo_Label").html(),
                angular.element("#ukUserQuestionnaireRISForm_BNo_Label").html()];
            let yesLabels = [
                angular.element("#ukUserQuestionnaireRISForm_AYes_Label").html(),
                angular.element("#ukUserQuestionnaireRISForm_BYes_Label").html()];
            let isSigned = vm.data.ukUserQuestionnaireRISForm.isSigned;
            let date = vm.data.ukUserQuestionnaireRISForm.date;
            let statement = {
                generalQuestion: generalQuestion,
                isSigned: isSigned,
                date: date
            };

            for (var i = 0; i < questions.length; ++i) {

                let question = questions[i];
                let answerIndex = answerIndexes[i];
                let yesExtraQuestion = yesExtraQuestions[i];
                let yesExtraAnswer = yesExtraAnswers[i];
                let noLabel = noLabels[i];
                let yesLabel = yesLabels[i];

                switch (answerIndex) {
                    case 0:
                        statement[question] = noLabel;
                        break;

                    case 1:
                        statement[question] = yesLabel;
                        statement[yesExtraQuestion] = yesExtraAnswer;
                        break;

                    default:
                        throw "Unknown answer: " + answerIndex + " question index:" + i;
                }
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
