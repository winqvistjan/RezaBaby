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

    // Google Map Start
    var myCenter = new google.maps.LatLng(60.172972, 24.941068);
    var hongKong = new google.maps.LatLng(22.288820, 114.177947);
    var cebu = new google.maps.LatLng(10.314343, 123.893673);
    var manila = new google.maps.LatLng(14.599484, 120.981711);
    var angeles = new google.maps.LatLng(15.145033, 120.586758);
    var singapore = new google.maps.LatLng(1.281437, 103.847699);
    var prague = new google.maps.LatLng(50.074435, 14.442278);

    function initialize() {
        var mapProp = {
            center: myCenter,
            zoom: 2,
            mapTypeId: google.maps.MapTypeId.ROADMAP
        };

        var map = new google.maps.Map(document.getElementById("googleMap"), mapProp);
        // Home Start
        var markerHome = new google.maps.Marker({
            position: myCenter,
        });

        markerHome.setMap(map);

        var infowindowHome = new google.maps.InfoWindow({
            content: "Helsinki"
        });

        infowindowHome.open(map, markerHome);
        // Home End

        // HongKong Start
        var markerHongKong = new google.maps.Marker({
            position: hongKong,
        });

        markerHongKong.setMap(map);       

        var infowindowHongKong = new google.maps.InfoWindow({
            content: "Hong Kong, 10.01.2015"
        });

        infowindowHongKong.open(map, markerHongKong);
        // HongKong End

        // Cebu Start
        var markerCebu = new google.maps.Marker({
            position: cebu,
        });

        markerCebu.setMap(map);

        var infowindowCebu = new google.maps.InfoWindow({
            content: "Cebu City, 11.01.2015"
        });

        infowindowCebu.open(map, markerCebu);
        // Cebu End

        // Manila Start
        var markerManila = new google.maps.Marker({
            position: manila,
        });

        markerManila.setMap(map);

        var infowindowManila = new google.maps.InfoWindow({
            content: "Manila, 20.01.2015"
        });

        infowindowManila.open(map, markerManila);
        // Manila End

        // Angeles Start
        var markerAngeles = new google.maps.Marker({
            position: angeles,
        });

        markerAngeles.setMap(map);

        var infowindowAngeles = new google.maps.InfoWindow({
            content: "Angeles, 22.01.2015"
        });

        infowindowAngeles.open(map, markerAngeles);
        // Angeles End

        // Singapore Start
        var markerSingapore = new google.maps.Marker({
            position: singapore,
        });

        markerSingapore.setMap(map);

        var infowindowSingapore = new google.maps.InfoWindow({
            content: "Singapore, 16.01.2015"
        });

        infowindowSingapore.open(map, markerSingapore);
        // Singapore End

        // Prague Start
        var markerPrague = new google.maps.Marker({
            position: prague,
        });

        markerPrague.setMap(map);

        var infowindowPrague = new google.maps.InfoWindow({
            content: "Prague, 30.06.2015"
        });

        infowindowPrague.open(map, markerPrague);
        // Prague End
    }

    google.maps.event.addDomListener(window, 'load', initialize);
    // Google Map End

    //// Google Map Start 2
    //// This example adds a search box to a map, using the Google Place Autocomplete
    //// feature. People can enter geographical searches. The search box will return a
    //// pick list containing a mix of places and predicted search terms.
    //function initAutocomplete() {
    //    var map = new google.maps.Map(document.getElementById('map'), {
    //        center: { lat: -33.8688, lng: 151.2195 },
    //        zoom: 13,
    //        mapTypeId: google.maps.MapTypeId.ROADMAP
    //    });

    //    // Create the search box and link it to the UI element.
    //    var input = document.getElementById('pac-input');
    //    var searchBox = new google.maps.places.SearchBox(input);
    //    map.controls[google.maps.ControlPosition.TOP_LEFT].push(input);

    //    // Bias the SearchBox results towards current map's viewport.
    //    map.addListener('bounds_changed', function () {
    //        searchBox.setBounds(map.getBounds());
    //    });

    //    var markers = [];
    //    // Listen for the event fired when the user selects a prediction and retrieve
    //    // more details for that place.
    //    searchBox.addListener('places_changed', function () {
    //        var places = searchBox.getPlaces();

    //        if (places.length == 0) {
    //            return;
    //        }

    //        // Clear out the old markers.
    //        markers.forEach(function (marker) {
    //            marker.setMap(null);
    //        });
    //        markers = [];

    //        // For each place, get the icon, name and location.
    //        var bounds = new google.maps.LatLngBounds();
    //        places.forEach(function (place) {
    //            var icon = {
    //                url: place.icon,
    //                size: new google.maps.Size(71, 71),
    //                origin: new google.maps.Point(0, 0),
    //                anchor: new google.maps.Point(17, 34),
    //                scaledSize: new google.maps.Size(25, 25)
    //            };

    //            // Create a marker for each place.
    //            markers.push(new google.maps.Marker({
    //                map: map,
    //                icon: icon,
    //                title: place.name,
    //                position: place.geometry.location
    //            }));

    //            if (place.geometry.viewport) {
    //                // Only geocodes have viewport.
    //                bounds.union(place.geometry.viewport);
    //            } else {
    //                bounds.extend(place.geometry.location);
    //            }
    //        });
    //        map.fitBounds(bounds);
    //    });
    //}
    //// Google Map End 2
});