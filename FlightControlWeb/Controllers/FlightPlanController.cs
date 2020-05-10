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
        private IMemoryCache _cache;
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

            flightplan.ID = flightID;
            IList<FlightPlan> flightplans;
            if (!_cache.TryGetValue("flightplans", out flightplans))
            {
                flightplans = new List<FlightPlan>();
                flightplans.Add(flightplan);
                _cache.Set("flightplans", flightplans);
            }
            else
            {
                flightplans.Add(flightplan);
            }

            return Ok(flightplan);
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
    }
}