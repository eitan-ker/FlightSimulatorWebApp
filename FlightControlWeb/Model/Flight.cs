using FlightControlWeb.Model;
using System;

namespace FlightControlWeb
{
    public class Flight
    {
        public Flight(string FlightID, double Longitude, double Latitude, int Passengers, string Company_name, DateTime Date_time, bool Is_external)
        {
            this.FlightID = FlightID;
            this.Longitude = Longitude;
            this.Latitude = Latitude;
            this.Passengers = Passengers;
            this.Company_name = Company_name;
            this.Date_time = Date_time;
            this.Is_external = Is_external;
        }
        public Flight(FlightPlan flightplan,bool is_external)
        {
            this.FlightID = flightplan.ID;
            this.Longitude = flightplan.Initial_Location.Longitude;
            this.Latitude = flightplan.Initial_Location.Latitude;
            this.Passengers = flightplan.Passengers;
            this.Company_name = flightplan.Company_Name;
            this.Date_time = flightplan.Initial_Location.Date_Time;
            this.Is_external = is_external;
        }
        public DateTime Date_time { get; set; }

        public string FlightID { get; set; }
        public int Passengers { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public string Company_name { get; set; }
        public bool Is_external { get; set; }
    }
}
