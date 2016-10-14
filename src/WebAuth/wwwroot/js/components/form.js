(function ($) {
    'use strict';

    var $fileUploads = $(".js-file-upload");

    $fileUploads.each(function (i, ele) {
        var $input = $(ele).find(".file-upload__input");
        var $file = $(ele).find(".file-upload__file");

        $input.change(function () {
            $file.css("display", "block");

            var fileName = this.value.split('\\').pop();
            var fileType = fileName.split('.').pop();

            var $fileName = $file.find(".file-upload__name");
            var $fileIcon = $file.find(".file-upload__icon");

            $fileName.text(fileName);
            $fileIcon.addClass("file-upload__icon_" + fileType);
        });
    });

    var $changableInputs = $(".js-input-change");

    $changableInputs.each(function () {
        this.oninput = changeInputState;
        this.onpropertychange = this.oninput;
        this.onchange = this.oninput;
    });

    function changeInputState() {
        var changable = $(this).data("changable");

        if (changable === true) {
            $(this).data("changable", false);

            changeNextButtonStepState();
        }
    }

    function changeNextButtonStepState() {
        var $nextStepButton = $("input[name*='NextStep']");
        var nextStepButtonClassList = $nextStepButton.attr('class').split(/\s+/);

        $.each(nextStepButtonClassList, function(index, item) {
            if (item === "disabled") {
                $nextStepButton.removeClass("disabled");
                $nextStepButton.prop('disabled', false);
            }
        });
    }

}(jQuery));