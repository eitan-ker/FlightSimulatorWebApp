using System.Collections.Generic;

namespace FlightControlWeb.Model
{
    interface IFlightManager
    {
        IEnumerable<Flight> GetAllFlights();
        Flight GetFlightById(string id);
        void AddFlight(Flight temp);
        void UpdateFlight(Flight temp);
        void deleteFlight(string id);
    }
}
