$(document).ready(function() {
    $(".requester button").click(function() {
        var button = $(this),
            content = button.parent().find(".content");

        button.attr("disabled", "disabled");
        content.html("<p>Requesting lock...</p>");

        $.get("/Home/RequestWithLock?requester=" + button.data("requester"),
            function(data) {
                content.html(data);

                button.removeAttr("disabled");
            });
    });
});