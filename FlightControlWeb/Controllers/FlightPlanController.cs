using FlightControlWeb.Commons;
using FlightControlWeb.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
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
        //POST: api/flightplan
        /*[Route("flightplan")]
        [HttpPost("{flightplan}")]
        // test post method with flightplan object from Postman
        public IActionResult AddFlightPlan(FlightPlan flightplan, bool isExternal = false)
        {
            string flightID = RandomGenerator.RandomString(6, true);
            Flight curflight = MakeFlight(flightplan, isExternal);
            //convert to UTC 
            flightplan.Initial_Location.Date_Time = flightplan.Initial_Location.Date_Time.ToUniversalTime();
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
        }*/


        [Route("flightplan")]
        [HttpPost("{flightplan}")]
        // post method with flightplan object from Postman, default is false because it comes from postman\file by client
        public async Task<ActionResult> AddFlightPlans(IEnumerable<FlightPlan> flightplansparameter, bool isExternal = false)
        {
            //loop through the collection given as a parameter
            foreach (var flightplan in flightplansparameter)
            {
                string flightID = RandomGenerator.RandomString(6, true);//generate a unique ID for flightplan
                Flight curflight = await Task.Run(() => MakeFlight(flightplan, isExternal));// make flight base on flight plan
                //convert to UTC 
                flightplan.Initial_Location.Date_Time = flightplan.Initial_Location.Date_Time.ToUniversalTime();
                flightplan.ID = flightID;
                curflight.FlightID = flightID;
                //check if object with name flightsplans exists in memcache. if not we create one
                if (!_cache.TryGetValue("flightplans", out IList<FlightPlan> flightplans))
                {
                    flightplans = new List<FlightPlan>
                {
                    flightplan
                };
                    Dictionary<string, Flight> flightsdictionary = new Dictionary<string, Flight>
                    {
                        { curflight.FlightID, curflight }
                    };
                    //add List in the name of flightplans and dictionary named flights to memcache
                    _cache.Set("flightplans", flightplans);
                    _cache.Set("flights", flightsdictionary);
                }
                else
                //flightsplans exists in memcache, therefore flights dictionary exist also so we can add flight to dictionary without hasitation
                {
                    flightplans.Add(flightplan);
                    _cache.TryGetValue("flights", out Dictionary<string, Flight> flights);
                    flights.Add(curflight.FlightID, curflight);
                }
            }
            return Ok();
        }


        //GET: api/flightplan/{id}
        [Route("FlightPlan/{id}")]
        [HttpGet("{id}")]
        //test Get method with id as a parameter in order to return flightplan object in the body of the response with the same id 
        public async Task<IActionResult> GetFlightPlanByID(string id)
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
        private Flight MakeFlight(FlightPlan flightplan, bool is_external)
        {
            Flight flight = new Flight(flightplan.ID, flightplan.Initial_Location.Longitude, flightplan.Initial_Location.Latitude,
                flightplan.Passengers, flightplan.Company_Name, flightplan.Initial_Location.Date_Time, is_external);
            return flight;
        }
    }
}