(function () {
    'use strict';

    var app = angular.module('app');

    app
    .directive('terms', function () {
        return {
            restrict: 'E',
            template: `
                <div class="lykke-terms">
                    By Signing Up, you Agree to the
                    <div>
                        <a target="_blank" href="https://www.lykke.com/terms_of_use">Terms & Condition</a>
                        and
                        <a target="_blank" href="https://www.lykke.com/privacy_policy">Privacy Policy</a>.
                    </div>
                </div>
            `
        }
    });
})();
