(function () {
    'use strict';

    var controllerId = 'userProfileController';
    angular.module('profileApp').controller(controllerId, ['userProfileService', 'Upload', userProfileController]);

    function userProfileController(userProfileService, fileUpload) {
        var vm = this;

        vm.data = {
            personalData: {},
            socialPrefix: {
                facebook: 'https://fb.com/',
                github: 'https://github.com/',
                twitter: 'https://twitter.com/'
            },
            loading: true,
            form: {
                origData: {},
                edit: false,
                agree: true,
                loading: false,
                avatarLoading: false
            }
        };

        vm.handlers = {
            edit: edit,
            cancelEdit: cancelEdit,
            isSaveDisabled: isSaveDisabled,
            saveProfile: saveProfile,

            uploadAvatar: uploadAvatar,
            deleteAvatar: deleteAvatar
        };

        activate();

        function activate() {
            userProfileService.getPersonalData().then(function(data) {
                vm.data.personalData = data;
                if (vm.data.personalData.facebook)
                    vm.data.personalData.facebook = vm.data.personalData.facebook.replace(vm.data.socialPrefix.facebook, '');

                if (vm.data.personalData.github)
                    vm.data.personalData.github = vm.data.personalData.github.replace(vm.data.socialPrefix.github, '');

                if (vm.data.personalData.twitter)
                    vm.data.personalData.twitter = vm.data.personalData.twitter.replace(vm.data.socialPrefix.twitter, '');

                vm.data.loading = false;
            });
        }

        function edit() {
            vm.data.form.origData = angular.copy(vm.data.personalData);
            vm.data.form.edit = true;
        }

        function cancelEdit() {
            vm.data.personalData = angular.copy(vm.data.form.origData);
            vm.data.form.edit = false;
        }

        function isSaveDisabled() {
            return !(vm.data.form.agree && vm.personalDataForm.$valid);
        }

        function saveProfile() {
            var data = angular.copy(vm.data.personalData);

            if (data.facebook)
                data.facebook = vm.data.socialPrefix.facebook + vm.data.personalData.facebook;

            if (data.github)
                data.github = vm.data.socialPrefix.github + vm.data.personalData.github;

            if (data.twitter)
                data.twitter = vm.data.socialPrefix.twitter + vm.data.personalData.twitter;

            vm.data.form.loading = true;
            userProfileService.savePersonalData(data).then(function (data) {
                vm.data.form.edit = false;
                vm.data.form.loading = false;
            });
        }

        function uploadAvatar(files) {
            if (files && files.length) {

                var file = files[0];
                vm.data.form.avatarLoading = true;
                fileUpload.upload({
                    url: '/profile/uploadAvatar',
                    file: file
                }).then(function (resp) {
                    if (resp.status === 200 && resp.data) {
                        vm.data.personalData.avatarUrl = resp.data;
                    }
                }).finally(function() {
                    vm.data.form.avatarLoading = false;
                });
            }
        }

        function deleteAvatar() {
            userProfileService.deleteAvatar().then(function (data) {
                vm.data.personalData.avatarUrl = null;
            });
        }
    }
})();
