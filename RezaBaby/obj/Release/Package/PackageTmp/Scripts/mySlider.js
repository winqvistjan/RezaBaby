$(document).ready(function () {
    $("#slideshow").css("overflow", "hidden");

    //$("ul#slides").cycle({
    //    fx: 'fade',
    //    pause: 1,
    //    prev: '#prev',
    //    next: '#next'
    //});
    $("ul#slides").cycle({
        fx: 'scrollDown'
    });

    $("ul#galleryPort").cycle({
        fx: 'fade',
        speed: 'slow',
        timeout: 4000,
        pause: 1,
        prev: '#prev',
        next: '#next'
    });

    $("#slideshow").hover(function () {
        $("ul#nav").fadeIn();
    },
       function () {
           $("ul#nav").fadeOut();
       });
});