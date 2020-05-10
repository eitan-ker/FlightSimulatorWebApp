using System.Collections.Generic;

namespace FlightControlWeb.Model
{
    public class FlightManager : IFlightManager
    {
        private static List<Flight> flights;

        public void AddFlight(Flight temp)
        {
            flights.Add(temp);
        }

        public void deleteFlight(string id)
        {

            /*Flight temp = flights.Where(x => x.FlightID == id).FirstOrDefault();
            if (temp == null)
            {
                throw new Exception("product not found");
            }
            flights.Remove(temp);*/
        }

        public IEnumerable<Flight> GetAllFlights()
        {
            return flights;
        }

        public Flight GetFlightById(string id)
        {
            /*Flight temp = flights.Where(x => x.FlightID == id).FirstOrDefault();
            return temp;*/
            return null;
        }

        public void UpdateFlight(Flight temp)
        {
            /*Flight _flight = flights.Where(x => x.FlightID == temp.FlightID).FirstOrDefault();
            _flight.Date_time = temp.Date_time;
            _flight.FlightID = temp.FlightID;
            _flight.Longitude = temp.Longitude;
            _flight.Latitude = temp.Latitude;
            _flight.Company_name = temp.Company_name;
            _flight.Is_external = temp.Is_external;*/
        }
    }
}
