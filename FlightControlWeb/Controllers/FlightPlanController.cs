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
            if (!_cache.TryGetValue("flightplans", out Dictionary<string, FlightPlan> flightplans))
            {
                flightplans = new Dictionary<string, FlightPlan>();
                flightplans.Add(flightID, flightplan);
                _cache.Set("flightplans", flightplans);
            }
            else
            {
                flightplans.Add(flightID, flightplan);
            }

            return Ok(flightplan);
        }
        //GET: api/flightplan/{id}
        [Route("FlightPlan/{id}")]
        [HttpGet("{id}")]
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

        private FlightPlan LookupFlightByCode(string id)
        {
            if (_cache.TryGetValue("flightplans", out Dictionary<string, FlightPlan> flightplans))
            {
                return flightplans[id];
            }
            return null;
        }
    }
}