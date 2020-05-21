let active_flights = [];
let allMarkers = [];
let flightPath = null;
let map = null;
(function () {
    window.addEventListener('load', () => {
       map = initMap();
        flightPath = new google.maps.Polyline({
            geodesic: true,
            strokeColor: '#FF0000',
            strokeOpacity: 1.0,
            strokeWeight: 2
        });
        init(map);
    });
})();

async function init(map) {
    active_flights = await getActiveFlights();
    if (active_flights) {     
        active_flights.forEach((flight) => {
            for (let i = 0; i < allMarkers.length; i = i + 1) {
                if (allMarkers[i].title == flight.company_name + '-' + flight.flightID) {
                    allMarkers[i].setMap(null);
                }
            }
            addFlight(flight, map);
        });
        showFlightList(active_flights);
    }
    setTimeout(init, 1000, map);
    //console.log(active_flights);
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
async function getActiveFlightplan(id) {
    
    //$.ajax(`https://localhost:44383/api/Flights?relative_to=${currentDate}`).done((data) => {
       //console.log(data);
    //})
    //should be: 2020-05-13T17:54:30
    let geturl = "/api/FlightPlan/" + id;
    let settingss = {
        "url": geturl,
        "method": "GET",
        "timeout": 0,
    };

    return await $.ajax(settingss);
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
        if (flightPath) {
            flightPath.setMap(null);
        }
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
function addFlight(flight, gmap) {
    const iconImage = '../images/plane.png';
    const marker = new google.maps.Marker({
        position: new google.maps.LatLng(flight.latitude, flight.longitude),
        map:gmap,
        icon: iconImage,
        title: flight.company_name + '-' + flight.flightID
        //check for custom icon
    })
    marker.addListener('click', async function () {
        // 1. you need to fetch the data from webapi of the flight plan
        showFlightDetailsByID(flight.flightID,gmap);
    });
    allMarkers.push(marker);
}
async function showFlightDetailsByID(flightId,gmap) {
    const curflightplan = await getActiveFlightplan(flightId);
    paintFlightPath(curflightplan, gmap);
    showFlightDetails(curflightplan)
    
}
function paintFlightPath(flightPlan, gmap) {
    const flightPlanCoordinates = [];
    flightPlanCoordinates.push({ lat: flightPlan.initial_Location.latitude, lng: flightPlan.initial_Location.longitude });
    flightPlan.segments.forEach((segment) => {
        flightPlanCoordinates.push({ lat: segment.latitude, lng: segment.longitude });
    });
    flightPath.setMap(null);
    flightPath.setPath(flightPlanCoordinates);
    flightPath.setMap(gmap);
}

function showFlightList(flightList) {
    $(".myflight-list").empty();
    const ul = document.createElement("ul");
    ul.classList.add("flights");
    flightList.forEach((flight) => {
        const li = document.createElement("li");
        li.innerHTML = `${flight.flightID} - ${flight.company_name} <a href="#">X</a>`;
        li.id = flight.flightID;
        ul.append(li);
    });
    $(".myflight-list").append(ul);

}
function showFlightDetails(flightplan) {
    $(".flights-details").empty();
    const table = document.createElement("table");
    table.border = "1";
    table.width="100%"
    const row = table.insertRow(0);
    const ID_header = row.insertCell(0);
    const fromLocation_header = row.insertCell(1);
    const toLocation_header = row.insertCell(2);
    const companyName_header = row.insertCell(3);
    const passengers_header = row.insertCell(4);
    ID_header.innerHTML = "Flight ID"
    fromLocation_header.innerHTML = "From Location";
    toLocation_header.innerHTML =   "To Location";
    companyName_header.innerHTML = "Company Name";
    passengers_header.innerHTML = "# of Passengers";
    const row2 = table.insertRow(1);
    const ID = row2.insertCell(0);
    const fromLocation = row2.insertCell(1);
    const toLocation = row2.insertCell(2);
    const companyName = row2.insertCell(3);
    const passengers = row2.insertCell(4);
    let flightplansegment = flightplan.segments;
    let final_destination = flightplansegment[flightplansegment.length - 1]; 
    let initial_location = flightplan.initial_Location;
    ID.innerHTML = flightplan.id;
    fromLocation.innerHTML = "Lat:" + initial_location.latitude + " Long:" + initial_location.longitude;
    toLocation.innerHTML = "Lat:" + final_destination.latitude + " Long:" + final_destination.longitude
    companyName.innerHTML = `<b>${flightplan.company_Name}</b>`;
    passengers.innerHTML = flightplan.passengers;
    $(".flights-details").append(table);
}
$(".myflight-list").on('click', (event) => {
    const flightID = event.target.id;
    if (flightID) {
        showFlightDetailsByID(flightID,map);
    }
    
})
