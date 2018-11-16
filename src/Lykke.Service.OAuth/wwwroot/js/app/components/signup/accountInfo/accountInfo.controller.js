(function () {
    'use strict';

    angular.module('app').controller('accountInfoController', accountInfoController);

    accountInfoController.$inject = ['signupService', '$scope', 'signupStep', '$dialogs', '$timeout', 'phoneErrorCode', 'page', 'signupEvent'];

    function accountInfoController(signupService, $scope, signupStep, $dialogs, $timeout, phoneErrorCode, page, signupEvent) {
        var vm = this;

        function handleSelectCountry() {
            var restrictedCountryFound = vm.data.restrictedCountries.find(function (restrictedCountry) {
                return restrictedCountry.iso2 === vm.data.model.country;
            });

            if (!!restrictedCountryFound) {
                openRestrictedCountryQuestionModal();
            }
        }

        function handleLogout() {
            signupService.signOut();
            $scope.$emit(signupEvent.currentStepChanged, signupStep.initialInfo);
            $scope.$emit(signupEvent.currentPageChanged, page.signIn);
        }

        function handleSubmit(form) {
            if (form.$valid && !vm.data.isSubmitting) {
                vm.data.isSubmitting = true;

                signupService.sendAccountInfo(
                    vm.data.model.firstName,
                    vm.data.model.lastName,
                    vm.data.model.country,
                    vm.data.model.phonePrefix + vm.data.model.phoneNumber
                ).then(function (data) {
                    vm.data.isSubmitting = false;
                    $scope.$emit(signupEvent.currentStepChanged, signupStep.pin);
                }).catch(function (error) {
                    vm.data.isSubmitting = false;

                    switch (error.data.error) {
                        case phoneErrorCode.invalidPhoneFormat:
                            vm.data.isPhoneFormatInvalid = true;
                            break;
                        case phoneErrorCode.phoneNumberInUse:
                            openPhoneInUseWarningModal();
                            break;
                    }
                });
            }
        }

        function handlePhoneKeydown() {
            vm.data.isPhoneFormatInvalid = false;
        }

        function openRestrictedCountryQuestionModal() {
            var userCountry = vm.data.countries.find(function (country) {
                return country.iso2 === vm.data.model.country;
            })

            $dialogs.showConfirmationDialog('', {
                title: 'Are you a citizen or resident of ' + userCountry.name + '?',
                buttonOkText: 'Yes',
                buttonCancelText : 'No',
                callback: function (option){
                    if (option === 'ok') {
                        openRestrictedCountryWarningModal();
                    } else {
                        vm.data.model.country = null;
                        $timeout(function () {
                            angular.element('.ui-select-toggle').click();
                        });
                    }
                }
            });
        }

        function openRestrictedCountryWarningModal() {
            var text = 'We are in the process of obtaining the necessary regulatory approvals for our services under your jurisdiction. Please note that we have your contact information in our database and will email you updates as they become available.';

            $dialogs.showInfoDialog(text, {
                title: 'Important Information',
                buttonCloseText: 'Ok, got it!',
                callback: function () {
                    vm.data.model.country = null;
                }
            });
        }

        function openPhoneInUseWarningModal() {
            var text = 'Looks like the phone number you entered is associated with another account. If you have trouble accessing this account please contact <a href="mailto:support@lykke.com">support@lykke.com</a>';

            $dialogs.showInfoDialog(text, {
                title: 'Oops…',
                buttonCloseText: 'Ok, got it!'
            });
        }

        vm.data = {
            isSubmitting: false,
            isPhoneFormatInvalid: false,
            model: {
                firstName: '',
                lastName: '',
                country: '',
                phonePrefix: '',
                phoneNumber: ''
            },
            countries: [],
            restrictedCountries: []
        }

        vm.handlers = {
            handleSubmit: handleSubmit,
            handleSelectCountry: handleSelectCountry,
            handlePhoneKeydown: handlePhoneKeydown,
            handleLogout: handleLogout
        };

        signupService.getCountries().then(function (response) {
            var defaultCountryIso2 = 'CH';
            var userCountryIso2 = response.userLocationCountry && response.userLocationCountry.iso2 || defaultCountryIso2;
            vm.data.countries = response.countries;
            vm.data.restrictedCountries = response.restrictedCountriesOfResidence;
            var userCountry = vm.data.countries.find(function (country) {
                return country.iso2 === userCountryIso2;
            })

            vm.data.model.country = userCountryIso2;
            vm.data.model.phonePrefix = userCountry.phonePrefix;
            handleSelectCountry();

            // Hack: Lib doesn't provide interface for this
            angular.element('.select2-search input').prop('placeholder', 'Search for your country or select one from the list');
        });
    }
})();
