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
    // Upload file validation Start

    // Get file size Start
    function GetFileSize() {
        try {
            var fileSize = 0;
            if (window.File && window.FileReader && window.FileList && window.Blob) {
                //get the file size from each file from file input field in kb
                var fileLenght = $('#fileToUpload')[0].files.length;

                for (var i = 0; i < fileLenght; i++) {
                    var singleFileSize = $('#fileToUpload')[0].files[i].size;
                    fileSize = fileSize + singleFileSize;
                }

                //convert size to mb
                fileSize = fileSize / 1048576;
            } else {
                alert("Please upgrade your browser, because your current browser lacks some new features we need!");
            }
            return fileSize;
            //throw new Error("Uups something went wrong. Sorry for that");
        }
        catch (err) {
            $("#errorMessage").html("Uups something went wrong. Sorry for that");
            return false;
        }
    }
    // Get file size End

    $("#btnSubmit").click(function () {
        if ($('#fileToUpload').val() == "") {
            $("#errorMessage").html("");
            //return false;
        }
        else {
            return checkfile();
        }
    });

    function checkfile() {
        var fileLenght = $('#fileToUpload')[0].files.length;

        for (var i = 0; i < fileLenght; i++) {
            var singleFileName = $('#fileToUpload')[0].files[i].name;
            
            if (singleFileName != null) {
                var extension = singleFileName.substr((singleFileName.lastIndexOf('.') + 1));

                switch (extension) {
                    case 'jpg':
                    case 'JPG':
                    case 'png':
                    case 'PNG':
                    case 'gif':
                    case 'GIF':
                    case 'mp4':
                    case 'MP4':
                        flag = true;
                        break;
                    default:
                        flag = false;
                }

                if (flag == false) {
                    $("#errorMessage").html("You can upload only jpg, png, gif, and mp4 extension file");
                    return false;
                }
            }
        }
        
        var size = GetFileSize();
        if (size > 100) {
            $("#errorMessage").html("You can upload file up to 100 MB");
            return false;
        }
        else {
            $("#errorMessage").html("");
        }
    }

    $(function () {
        $("#fileToUpload").change(function () {
            checkfile();
        });
    });
    // Get file path from client system End

    // Upload file validation End
});