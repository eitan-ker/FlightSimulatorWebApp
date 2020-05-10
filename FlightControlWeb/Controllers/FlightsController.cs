using FlightControlWeb.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

namespace FlightControlWeb.Controllers
{
    [Route("api")]
    [ApiController]
    public class FlightsController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        public FlightsController(IMemoryCache cache)
        {
            this._cache = cache;
        }
        //DELETE: flights/{id}
        [Route("flights/{id}")]
        [HttpDelete("{id}")]
        public IActionResult DeleteFlightByID(string id)
        {
            if (_cache.TryGetValue("flightplans", out List<FlightPlan> flightplans))
            {
                foreach (var it in flightplans)
                {
                    if (string.Compare(it.ID, id) == 0)
                    {
                        flightplans.Remove(it);
                        _cache.TryGetValue("flights", out Dictionary<string, Flight> flights);
                        flights.Remove(id);
                        return Ok();
                    }
                }
            }
            return NotFound();
        }
        [Route("flights")]
        [HttpGet("{Date}")]
        public IList<Flight> GetFlightByDate(DateTime relative_to, bool sync_all = false)
        {
            List<Flight> flightslist = new List<Flight>();
            if (sync_all)
            {
                if (_cache.TryGetValue("flightplans", out List<FlightPlan> flightplans))
                {
                    _cache.TryGetValue("flights", out Dictionary<string, Flight> flights);
                    foreach (var it in flightplans)
                    {
                        if(DateTime.Compare(DateTime.Now,it.Initial_Location.Date_Time)==0)
                        {
                            if (flights.ContainsKey(it.ID))
                            {
                                flightslist.Add(flights[it.ID]);
                            }
                        }
                    }
                }
            } else
            {
                if (_cache.TryGetValue("flightplans", out List<FlightPlan> flightplans))
                {
                    _cache.TryGetValue("flights", out Dictionary<string, Flight> flights);
                    foreach (var it in flightplans)
                    {
                        if (DateTime.Compare(DateTime.Now, it.Initial_Location.Date_Time) == 0)
                        {
                            if (flights.ContainsKey(it.ID) && flights[it.ID].Is_external == false)
                            {
                                flightslist.Add(flights[it.ID]);
                            }
                        }
                    }
                }
            }
            return flightslist;
        }
    }
}