using FlightControlWeb.Commons;
using FlightControlWeb.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlightControlWeb.Controllers
{
    [Route("api")]
    [ApiController]
    public class FlightPlanController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        public FlightPlanController(IMemoryCache cache)
        {
            this._cache = cache;
        }


        [Route("flightplan")]
        [HttpPost("{flightplan}")]
        // post method with flightplan object from Postman, default is false because it comes from postman\file by client
        public async Task<ActionResult> AddFlightPlans(FlightPlan flightplan,
            bool isExternal = false)
        {
            string flightId = RandomGenerator.RandomString(6, true);//generate a unique ID for flightplan
            Flight curflight = await Task.Run(() => MakeFlight(flightplan, isExternal));
            //convert to UTC 
            flightplan.Initial_Location.Date_Time = await Task.Run(() =>
            flightplan.Initial_Location.Date_Time.ToUniversalTime());
            flightplan.ID = flightId;
            curflight.FlightID = flightId;
            AddFlightPlan(flightplan, curflight);
            return Ok();
        }
        
        void AddFlightPlan(FlightPlan flightplan, Flight curflight)
        {
            //check if object with name flightsplans exists in memcache. if not we create one
            if (!_cache.TryGetValue("flightplans", out IList<FlightPlan> flightplans))
            {
                flightplans = new List<FlightPlan> { flightplan };
                Dictionary<string, Flight> flightsdictionary = new Dictionary<string, Flight>
                    { { curflight.FlightID, curflight } };
                //add List in the name of flightplans and dictionary named flights to memcache
                _cache.Set("flightplans", flightplans);
                _cache.Set("flights", flightsdictionary);
            }
            else
            //flightsplans exists in memcache, therefore flights dictionary exist also so we can 
            // add flight to dictionary without hasitation
            {
                flightplans.Add(flightplan);
                _cache.TryGetValue("flights", out Dictionary<string, Flight> flights);
                if (flights != null)
                {
                    flights.Add(curflight.FlightID, curflight);
                }
                else
                {
                    Dictionary<string, Flight> newFlight = new Dictionary<string, Flight>()
                    { { curflight.FlightID, curflight} };
                    _cache.Set("flights", newFlight);
                }
            }
        }

        //GET: api/flightplan/{id}
        [Route("FlightPlan/{id}")]
        [HttpGet("{id}")]
        //test Get method with id as a parameter in order to return flightplan object in the body of the response with the same id 
        public async Task<IActionResult> GetFlightPlanById(string id)
        {
            FlightPlan flightplan = await Task.Run(() => LookupFlightByCode(id));

            if (flightplan != null)
            {
                return Ok(flightplan);
            }
            else
            {
                return NotFound();
            }
        }




        //helper function with algorithm which generates unique id for testplan
        private FlightPlan LookupFlightByCode(string id)
        {
            if (_cache.TryGetValue("flightplans", out List<FlightPlan> flightplans))
            {
                return flightplans.Find(f => f.ID == id);

            }
            return null;
        }
        //helper function to create Flight object from FlightPlan object
        private Flight MakeFlight(FlightPlan flightplan, bool isExternal)
        {
            Flight flight = new Flight(flightplan.ID, flightplan.Initial_Location.Longitude,
                flightplan.Initial_Location.Latitude,
                flightplan.Passengers, flightplan.Company_Name,
                flightplan.Initial_Location.Date_Time, isExternal);
            return flight;
        }
    }
}