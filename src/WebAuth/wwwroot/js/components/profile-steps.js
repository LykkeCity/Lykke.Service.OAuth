(function ($) {
    'use strict';

    var mql = window.matchMedia('(max-width: 1100px)');
    var $stepItems = $(".profile-steps__item");

    function checkMobile() {
        var isMobile = mql.matches;

        if (isMobile) {
            changeProfileStepsView();
        } else {
            $stepItems.css("display", "block");
        }
    }

    mql.addListener(checkMobile);
    checkMobile(mql);

    function changeProfileStepsView() {

        var activeItemIndex = $stepItems.index($(".profile-steps__item_active"));
        var carouselLength = $stepItems.length;

        if (activeItemIndex == 0) {
            //show 123
            $stepItems.slice(activeItemIndex + 3).css("display", "none");
        } else if (activeItemIndex == carouselLength - 1) {
            //show 456
            $stepItems.slice(0, activeItemIndex - 2).css("display", "none");
        } else {
            $stepItems.slice(0, activeItemIndex - 1).css("display", "none");
            $stepItems.slice(activeItemIndex + 2, carouselLength).css("display", "none");
        }
    }
}(jQuery));