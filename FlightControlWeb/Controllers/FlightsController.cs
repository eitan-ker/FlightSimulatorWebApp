using FlightControlWeb.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

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
        [HttpGet]
        private void LinearInterpolation(FlightPlan flight, DateTime utcDate)
        {
            var segments = flight.Segments.ToList();
            int totalTimeSpan = segments.Sum(v => v.Timespan_seconds);
            TimeSpan dif = utcDate.Subtract(flight.Initial_Location.Date_Time);
            int dif_sec = (int)dif.TotalSeconds;
            if (totalTimeSpan > dif_sec)
            {
                double cameFromLati = flight.Initial_Location.Latitude;
                double cameFromLong = flight.Initial_Location.Longitude;
                foreach (Segment element in flight.Segments)
                {

                    if (dif_sec >= element.Timespan_seconds)
                    {
                        dif_sec = dif_sec - element.Timespan_seconds;
                    }
                    else
                    {
                        double relative = (double)dif_sec / (double)element.Timespan_seconds;
                        var flights = (IDictionary<string, Flight>)_cache.Get("flights");
                        flights[flight.ID].Latitude = cameFromLati + (relative * (element.Latitude - cameFromLati));
                        flights[flight.ID].Longitude = cameFromLong + (relative * (element.Longitude - cameFromLong));
                        break;
                    }
                    cameFromLati = element.Latitude;
                    cameFromLong = element.Longitude;
                }
            }

        }
        [Route("flights")]
        [HttpGet]
        public IList<Flight> GetFlightByDate(DateTime relative_to)
        {
            bool sync_all = Request.Query.ContainsKey("sync_all");
            List<Flight> flightslist = new List<Flight>();
            DateTime utcDate = relative_to.ToUniversalTime();
            if (_cache.TryGetValue("flightplans", out List<FlightPlan> flightplans))
            {

                List<FlightPlan> nonPlannedFlights = flightplans.FindAll(g => g.Initial_Location.Date_Time <= utcDate);
                nonPlannedFlights.ForEach(flight =>
                {
                    var segments = flight.Segments.ToList();
                    int totalTimeSpan = segments.Sum(v => v.Timespan_seconds);
                    if (flight.Initial_Location.Date_Time.AddSeconds(totalTimeSpan) > utcDate)
                    {
                        if (_cache.TryGetValue("flights", out Dictionary<string, Flight> allFlights))
                        {
                            if (!sync_all)
                            {
                                if (!allFlights[flight.ID].Is_external)
                                {
                                    flightslist.Add(allFlights[flight.ID]);
                                }

                            }
                            else
                            {
                                flightslist.Add(allFlights[flight.ID]);
                            }
                            LinearInterpolation(flight, utcDate);
                        }
                    }
                });

                return flightslist;
            }
            return null;
        }
    }
}