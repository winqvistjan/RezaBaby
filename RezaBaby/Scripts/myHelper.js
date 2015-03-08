$(function () {
    // Growth Start
    $("#more1").click(function () {
        $("#growth1").fadeOut(1000);

        setTimeout(function () {
            $("#growth2").fadeIn("slow");
        }, 2000);
        
    });
    $("#back1").click(function () {
        $("#growth2").fadeOut(1000);

        setTimeout(function () {
            $("#growth1").fadeIn("slow");
        }, 2000);
    });
    // Growth End
    // Gallery Start
    $("#gallery2").click(function () {
        $("#GalleryPortrate").fadeOut(1000);

        setTimeout(function () {
            $("#GalleryLandscape").fadeIn("slow");
        }, 2000);

    });
    $("#gallery1").click(function () {
        $("#GalleryLandscape").fadeOut(1000);

        setTimeout(function () {
            $("#GalleryPortrate").fadeIn("slow");
        }, 2000);
    });
    // Gallery End
});