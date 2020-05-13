let active_flights = [];

(function () {
    window.addEventListener('load', () => {

        const map = initMap();
        init(map);
    });
})();

async function init(map) {
    active_flights = await getActiveFlights();
    active_flights.forEach((flight) => {
        addFlight(flight,map);
    });
    console.log(active_flights);
}

function formatDate(date) {
    let d = new Date(date),
        month = '' + (d.getMonth() + 1),
        day = '' + d.getDate(),
        year = d.getFullYear(),
        hours = d.getHours(),
        minutes = d.getMinutes(),
        seconds = d.getSeconds();

    if (month.length < 2)
        month = '0' + month;
    if (day.length < 2)
        day = '0' + day;
    if (hours < 10)
        hours = '0' + hours;
    if (minutes < 10)
        minutes = '0' + minutes;
    if (seconds < 10)
        seconds = '0' + seconds;
    //you need to add T between the dates and the hours and add ":" 

    let resultDate = [year, month, day].join('-');
    resultDate += 'T';
    resultDate += [hours, minutes, seconds].join(':');
    // return 
    return resultDate;
    // return [year, month, day,hours,minutes,seconds].join('-');
}

async function getActiveFlights() {
    const currentDate = formatDate(new Date());
    
    //$.ajax(`https://localhost:44383/api/Flights?relative_to=${currentDate}`).done((data) => {
       //console.log(data);
    //})
    //should be: 2020-05-13T17:54:30
    let url = "https://localhost:44383/api/Flights?relative_to=" + currentDate;
    let settings = {
        "url": url,
        "method": "GET",
        "timeout": 0,
    };

    return await $.ajax(settings);
}
function initMap() {
    //Map options
    const options = {

        center: { lat: 0, lng: 0 },
        zoom: 2,
        minZoom: 1
    }

    //new Map
    const map = new google.maps.Map(document.getElementById('map'), options);
    map.addListener('click', (e) => {
        $(".flights-details").empty();
    })
    return map;
}
////Add marker
//const image = 'https://developers.google.com/maps/documentation/javascript/examples/full/images/beachflag.png';
//const marker = new google.maps.Marker({
//    position: { lat: 32.0055, lng: 34.8854 },
//    map: map,
//    icon: image
//});


//Add Marker Function
function addFlight(flight,gmap) {
    const iconImage = '../images/plane.png';
    const marker = new google.maps.Marker({
        position: new google.maps.LatLng(flight.latitude, flight.longitude),
        map:gmap,
        icon: iconImage,
        title: flight.company_name + '-' + flight.flightID
        //check for custom icon
    })
    marker.addListener('click', function () {
       // 1. you need to fetch the data from webapi of the flight plan
        showFlightDetails(flight);
    });
   
}

function showFlightList() {
    //build a ul element
    //get the flights from server  flight id + company name
    // //for each flight, run on the array and create a li element
    //the li element will show the company name and flight id

}
function showFlightDetails(flight) {
    $(".flights-details").empty();
    const table = document.createElement("table");
    table.border = "1";
    table.width="100%"
    const row = table.insertRow(0);
    const fromLocation_header = row.insertCell(0);
    const toLocation_header = row.insertCell(1);
    const companyName_header = row.insertCell(2);
    const passengers_header = row.insertCell(3);
    fromLocation_header.innerHTML = "From Location";
    toLocation_header.innerHTML =   "To Location";
    companyName_header.innerHTML = "Company Name";
    passengers_header.innerHTML = "# of Passengers";
    const row2 = table.insertRow(1);
    const fromLocation = row2.insertCell(0);
    const toLocation = row2.insertCell(1);
    const companyName = row2.insertCell(2);
    const passengers = row2.insertCell(3);
    fromLocation.innerHTML = "Lat:" + flight.latitude + " Long:" + flight.longitude;
    toLocation.innerHTML = "TBD";
    companyName.innerHTML = `<b>${flight.company_name}</b>`;
    passengers.innerHTML = flight.passengers;
    $(".flights-details").append(table);

}
Dropzone.options.myDropzone = {
    dictDefaultMessage: "Drag & drop images here to upload",
    init: function () {
        var myDropzone = this;

        this.on("drop", function (event) {
            var imageUrl = event.dataTransfer.getData('URL');
            var fileName = imageUrl.split('/').pop();

            // set the effectAllowed for the drag item
            event.dataTransfer.effectAllowed = 'copy';

            function getDataUri(url, callback) {
                var image = new Image();

                image.onload = function () {
                    var canvas = document.createElement('canvas');
                    canvas.width = this.naturalWidth; // or 'width' if you want a special/scaled size
                    canvas.height = this.naturalHeight; // or 'height' if you want a special/scaled size

                    canvas.getContext('2d').drawImage(this, 0, 0);

                    // Get raw image data
                    // callback(canvas.toDataURL('image/png').replace(/^data:image\/(png|jpg);base64,/, ''));

                    // ... or get as Data URI
                    callback(canvas.toDataURL('image/jpeg'));
                };

                image.setAttribute('crossOrigin', 'anonymous');
                image.src = url;
            }

            function dataURItoBlob(dataURI) {
                var byteString,
                    mimestring

                if (dataURI.split(',')[0].indexOf('base64') !== -1) {
                    byteString = atob(dataURI.split(',')[1])
                } else {
                    byteString = decodeURI(dataURI.split(',')[1])
                }

                mimestring = dataURI.split(',')[0].split(':')[1].split(';')[0]

                var content = new Array();
                for (var i = 0; i < byteString.length; i++) {
                    content[i] = byteString.charCodeAt(i)
                }

                return new Blob([new Uint8Array(content)], {
                    type: mimestring
                });
            }

            getDataUri(imageUrl, function (dataUri) {
                var blob = dataURItoBlob(dataUri);
                blob.name = fileName;
                myDropzone.addFile(blob);
            });
        });

    } // init
} // Dropzone