(function () {
    'use strict';

    angular.module('app').controller('accountInfoController', accountInfoController);

    accountInfoController.$inject = ['signupService', '$scope', 'signupStep', '$dialogs', '$timeout'];

    function accountInfoController(signupService, $scope, signupStep, $dialogs, $timeout) {
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
                    $scope.$emit('currentStepChanged', signupStep.pin);
                }).catch(function (error) {
                    vm.data.isSubmitting = false;
                    // TODO: Password exists
                });
            }
        }

        function openRestrictedCountryQuestionModal() {
            var userCountry = vm.data.countries.find(function (country) {
                return country.iso2 === vm.data.model.country;
            })

            $dialogs.showConfirmationDialog('', {
                title: 'Are you a citizen or resident of the ' + userCountry.name + '?',
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

        vm.data = {
            isSubmitting: false,
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
            handleLogout: handleLogout
        };

        signupService.getCountries().then(function (response) {
            var userCountry;
            var userCountryIso2 = response.userLocationCountry && response.userLocationCountry.iso2;
            vm.data.countries = response.countries;
            vm.data.restrictedCountries = response.restrictedCountriesOfResidence;
            if (!userCountryIso2) {
                userCountryIso2 = vm.data.countries[0].iso2
            }
            userCountry = vm.data.countries.find(function (country) {
                return country.iso2 === userCountryIso2;
            })

            vm.data.model.country = response.userLocationCountry && response.userLocationCountry.iso2;
            vm.data.model.phonePrefix = userCountry.phonePrefix;
        });
    }
})();
