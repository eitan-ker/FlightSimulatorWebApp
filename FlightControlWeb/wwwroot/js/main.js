
(function () {
    const map = initMap();
    getActiveFlights();
})();

function formatDate(date) {
    var d = new Date(date),
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
    if (hours.length < 2)
        hours = '0' + hours;
    if (minutes.length < 2)
        minutes = '0' + minutes;
    if (seconds.length < 2)
        seconds = '0' + seconds;
    //you need to add T between the dates and the hours and add ":" 
    return [year, month, day,hours,minutes,seconds].join('-');
}
function getActiveFlights() {
    const currentDate = formatDate(new Date());
    $.ajax(`https://localhost:44383/api/Flights?relative_to=${currentDate}`).done((data) => {
        console.log(data);
    })
}
function initMap() {
    //Map options
    const options = {
       
        center: new google.maps.LatLng(0, 0),
        zoom: 2,
        minZoom: 1
    }

    //new Map
    const map = new google.maps.Map(document.getElementById('map'), options);
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
function addFlight(props) {
    const iconImage = 'https://developers.google.com/maps/documentation/javascript/examples/full/images/beachflag.png';
    const marker = new google.maps.Marker({
        position: props.coords,
        map: this.map,
        icon: iconImage
        //check for custom icon
    })
    if (props.IconImage) {
        marker.SetIcon(props.IconImage)
    }
    }
