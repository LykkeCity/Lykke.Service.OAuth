(function () {
    'use strict';

    angular
        .module('profileApp')
        .service('userProfileService', userProfileService);

    userProfileService.$inject = ['$http'];

    function userProfileService($http) {

        function getPersonalData() {
            return $http.post('/profile/getpersonaldata')
                .then(function (data) {
                    return data.data;
                });
        }

        function savePersonalData(profile) {
            return $http.post('/profile/savepersonaldata', profile)
                .then(function (data) {
                    return data.data;
                });
        }

        function deleteAvatar() {
            return $http.post('/profile/deleteavatar')
                .then(function (data) {
                    return data.data;
                });
        }

        return {
            getPersonalData: getPersonalData,
            savePersonalData: savePersonalData,
            deleteAvatar: deleteAvatar
        }
    }
})();
