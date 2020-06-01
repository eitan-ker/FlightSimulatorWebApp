let active_flights = [];
let allMarkers = [];
let flightPath = null;
let map = null;
let selectedFlightID = null;
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

//add marker on map and show flight details of the active flights
async function init(map) {
    let check_if_selected_flight_is_still_active = false;
    try {
        active_flights = await getActiveFlights();
        if (active_flights) {
            removemarkertrails();
            active_flights.forEach((flight) => {
                //check if selectedflight is still active, if not then the boolean will turn to true
                if (selectedFlightID != null && selectedFlightID == flight.flightID && check_if_selected_flight_is_still_active == false) {
                    check_if_selected_flight_is_still_active = true;
                }
                addFlight(flight, map);
            });
            showFlightList(active_flights);
            //check if marked flight is no longer active, if so then delete its path on map and its flight details
            if (check_if_selected_flight_is_still_active == false) {
                paintFlightPath(null, map);
                $(".flights-details").empty();
            }
        }
        //recursive call for init every 1 sec
        setTimeout(init, 1000, map);
    }
    catch (err) {
        if (err) {
            if (err.statusText === 'error') {
                toastr.error("Server is down");
            } else {
                toastr.error("Error " + err.statusText);
            }
           
        } else {
            toastr.error("Something when wrong");
        }
       
    }
    //console.log(active_flights);
}
//remove current markers on map before new flights get request comes active
function removemarkertrails() {
    for (let i = 0; i < allMarkers.length; i = i + 1) {
        /*if (allMarkers[i].title == flight.company_name + '-' + flight.flightID) {
            allMarkers[i].setMap(null);
        }*/
        allMarkers[i].setMap(null);
    }
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
//get active flights from server
async function getActiveFlights() {
    const currentDate = formatDate(new Date());
    
    //$.ajax(`/api/Flights?relative_to=${currentDate}`).done((data) => {
       //console.log(data);
    //})
    //should be: 2020-05-13T17:54:30
    let url = "/api/Flights?relative_to=" + currentDate +"&"+'sync_all';
    let settings = {
        "url": url,
        "method": "GET",
        "timeout": 0,
    };

    return await $.ajax(settings);
}
//get active flightplan by id
async function getActiveFlightplan(id) {
    
    //$.ajax(`/api/Flights?relative_to=${currentDate}`).done((data) => {
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
//initialize map 
function initMap() {
    //Map options
    const options = {

        center: { lat: 0, lng: 0 },
        zoom: 2,
        minZoom: 1
    };

    //new Map
    const map = new google.maps.Map(document.getElementById('map'), options);
    map.addListener('click', (e) => {
        $(".flights-details").empty();
        selectedFlightID = null;
        removeSelectedFlights();
        if (flightPath) {
            flightPath.setMap(null);
        }
    });

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
    const iconImage = flight.flightID !== selectedFlightID ? '../images/plane.png' : '../images/yellowaircraft.png';
    const marker = new google.maps.Marker({
        position: new google.maps.LatLng(flight.latitude, flight.longitude),
        map:gmap,
        icon: iconImage,
        title: flight.company_name + '-' + flight.flightID
        //check for custom icon
    });
    marker.addListener('click', async function () {
        //marker.icon = 'https://developers.google.com/maps/documentation/javascript/examples/full/images/beachflag.png';
        selectedFlightID = flight.flightID;
        showFlightDetailsByID(flight.flightID,gmap);
    });
    allMarkers.push(marker);
}
//get flightplan details to show on bar from the right of the page and draw its course on map
async function showFlightDetailsByID(flightId, gmap) {
    try {
        const curflightplan = await getActiveFlightplan(flightId);
        paintFlightPath(curflightplan, gmap);
        showFlightDetails(curflightplan);
    }
     catch (err) {
        if (err) {
            if (err.statusText === 'error') {
                toastr.error("Server is down");
            } else {
                toastr.error("Error " + err.statusText);
            }
        } else {
            toastr.error("Something when wrong");
        }

    }
}
//draw flightplan track on map
function paintFlightPath(flightPlan, gmap) {
    const flightPlanCoordinates = [];
    if (flightPlan) {
        flightPlanCoordinates.push({ lat: flightPlan.initial_Location.latitude, lng: flightPlan.initial_Location.longitude });
        flightPlan.segments.forEach((segment) => {
            flightPlanCoordinates.push({ lat: segment.latitude, lng: segment.longitude });
        });
    }
    flightPath.setMap(null);
    flightPath.setPath(flightPlanCoordinates);
    flightPath.setMap(gmap);
}
//prepare a list of active internal flights and external flights seperately from server
async function showFlightList(flightList) {
    let internalFlights = await getinternalFLights(flightList);
    if (internalFlights) {
        showFlightListByClass(internalFlights,".myflight-list");
    }
    let externalFlights = await getexternalFLights(flightList);
    if (externalFlights) {
        showFlightListByClass(externalFlights,".exflight-list");
    }
   //let internalFLights = await showinternalFlightList(flightList);
    //showexternalFlightList(flightList);
}
//get all active internal flights from server
function getinternalFLights(flightlist) {
    let internalFlights = [];
    if (flightlist) {
        flightlist.forEach((flight) => {
            if (flight.is_external == false) {
                internalFlights.push(flight);
            }
        });
    }
    /*for (var flight in flightlist) {
        if (flight.is_external == false) {
                internalFlights.push(flight);
          }
    }*/
    return internalFlights;
}
//get all active external flights from server
function getexternalFLights(flightlist) {
    let externalFlights = [];
    if (flightlist) {
        flightlist.forEach((flight) => {
            if (flight.is_external == true) {
                externalFlights.push(flight);
            }
        });
    }
    return externalFlights;
}
//show external flights in my flights bar from the right of the page
function showFlightListByClass(flightList,classFlightList) {
    $(classFlightList).empty();
    const ul = document.createElement("ul");
   // ul.classList.add("exflights");
    flightList.forEach((flight) => {
        const li = document.createElement("li");
        if (selectedFlightID === flight.flightID) {
            li.classList.add('selected');
        }
        let curflightID = flight.flightID;
        if (classFlightList === ".exflight-list") {
            li.innerHTML = `${flight.flightID} - ${flight.company_name}`;
        } else {
            li.innerHTML = `${flight.flightID} - ${flight.company_name} <a onclick="deleteflightAfterPressingX('${curflightID}');" href="#">X</a>`;
        }
        li.id = flight.flightID;
        ul.append(li);
    });
    $(classFlightList).append(ul);
}
//after X button is pressed , it will delete flight from DB and map
async function deleteflightAfterPressingX(id) {
    let deleteurl = "/api/Flights/" + id;
    let settingss = {
        "url": deleteurl,
        "method": "DELETE",
        "timeout": 0,
    };
    try {
        await $.ajax(settingss);
        if (id === selectedFlightID) {
            $(".flights-details").empty();
            paintFlightPath(null, map);
        }
    }
    catch (err) {
        toastr.error("error in deleting the flight from DB, server return error 404 which means that there is no flight with that flightID");
    }
  
}
//show flight details in flight details bar at the bottom of the page
function showFlightDetails(flightplan) {
    $(".flights-details").empty();
    const table = document.createElement("table");
    table.border = "1";
    table.width = "100%";
    table.classList.add("table");
  
    const tHeader = table.createTHead();
    const row = tHeader.insertRow(0);

   
    const ID_header = document.createElement("th");
    const fromLocation_header = document.createElement("th");
    const toLocation_header = document.createElement("th");
    const companyName_header = document.createElement("th");
    const passengers_header = document.createElement("th");
    ID_header.innerHTML = "Flight ID";
    row.appendChild(ID_header);
    fromLocation_header.innerHTML = "From Location";
    row.appendChild(fromLocation_header);
    toLocation_header.innerHTML = "To Location";
    row.appendChild(toLocation_header);
    companyName_header.innerHTML = "Company Name";
    row.appendChild(companyName_header);
    passengers_header.innerHTML = "# of Passengers";
    row.appendChild(passengers_header);
    const row2 = table.createTBody().insertRow(0);
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
    toLocation.innerHTML = "Lat:" + final_destination.latitude + " Long:" + final_destination.longitude;
    companyName.innerHTML = `<b>${flightplan.company_Name}</b>`;
    passengers.innerHTML = flightplan.passengers;
    $(".flights-details").append(table);
    removeSelectedFlights();
    $(`#${flightplan.id}`).addClass("selected");
}
function removeSelectedFlights() {
    $("ul li.selected").removeClass("selected");
}

$(".myflight-list, .exflight-list").on('click',
    (event) => {
        const flightID = event.target.id;
        if (flightID) {
            selectedFlightID = flightID;
            showFlightDetailsByID(flightID, map);
        }

    });
