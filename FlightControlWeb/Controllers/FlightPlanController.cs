using FlightControlWeb.Commons;
using FlightControlWeb.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

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
        //POST: api/flightplan
        [Route("flightplan")]
        [HttpPost("{flightplan}")]
        // test post method with flightplan object from Postman
        public IActionResult AddFlightPlan(FlightPlan flightplan)
        {
            string flightID = RandomGenerator.RandomString(6, true);
            Flight curflight = MakeFlight(flightplan, false);

            flightplan.ID = flightID;
            curflight.FlightID = flightID;
            if (!_cache.TryGetValue("flightplans", out IList<FlightPlan> flightplans))
            {
                flightplans = new List<FlightPlan>
                {
                    flightplan
                };
                Dictionary<string, Flight> flightsdictionary = new Dictionary<string, Flight>();
                flightsdictionary.Add(curflight.FlightID, curflight);
                _cache.Set("flightplans", flightplans);
                _cache.Set("flights", flightsdictionary);
            }
            else
            {
                flightplans.Add(flightplan);
                _cache.TryGetValue("flights", out Dictionary<string, Flight> flights);
                flights.Add(curflight.FlightID, curflight);
            }

            return Ok(curflight);
        }
        //GET: api/flightplan/{id}
        [Route("FlightPlan/{id}")]
        [HttpGet("{id}")]
        //test Get method with id as a parameter in order to return flightplan object in the body of the response with the same id 
        public IActionResult GetFlightPlanByID(string id)
        {
            FlightPlan flightplan = LookupFlightByCode(id);

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
                foreach(var it in flightplans)
                {
                    if(string.Compare(it.ID, id) == 0)
                    {
                        return it;
                    }
                }   
            }
            return null;
        }
        //helper function to create Flight object from FlightPlan object
        private Flight MakeFlight(FlightPlan flightplan, bool is_external)
        {
            Flight flight = new Flight(flightplan.ID, flightplan.Initial_Location.Longitude, flightplan.Initial_Location.Latitude,
                flightplan.Passengers, flightplan.Company_Name, flightplan.Initial_Location.Date_Time, is_external);
            return flight;
        }
    }
}