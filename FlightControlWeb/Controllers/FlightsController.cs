using FlightControlWeb.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<IActionResult> DeleteFlightByID(string id)
        {
            //check if data structure name "flightplans" exist in memcache , create it
            if (_cache.TryGetValue("flightplans", out List<FlightPlan> flightplans))
            {
                /*loop throgh flight plans in datasctruture and find the requested flightplan, if it exist then delete it from the flightplans 
                 * data structure, remove both flightplan and respective flight*/
                foreach (var it in flightplans)
                {
                    //remove b
                    if (string.Compare(it.ID, id) == 0)
                    {
                        await Task.Run(() => flightplans.Remove(it));
                        _cache.TryGetValue("flights", out Dictionary<string, Flight> flights);
                        await Task.Run(() => flights.Remove(id));
                        return Ok();
                    }
                }
            }
            //there isnt a datastructure name "flightplans" in memcache
            return NotFound();
        }
        //linear interpolation function to calculate relative distance the plane flew to the destination of the current segment
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
                        dif_sec -= element.Timespan_seconds;
                    }
                    else
                    {
                        double relative = (double)dif_sec / (double)element.Timespan_seconds;
                        var flights = (IDictionary<string, Flight>)_cache.Get("flights");
                        if (flights.ContainsKey(flight.ID))
                        {
                            flights[flight.ID].Latitude = cameFromLati + (relative * (element.Latitude - cameFromLati));
                            flights[flight.ID].Longitude = cameFromLong + (relative * (element.Longitude - cameFromLong));
                        }
                        break;
                    }
                    cameFromLati = element.Latitude;
                    cameFromLong = element.Longitude;
                }
            }

        }
        [Route("flights")]
        [HttpGet]
        //get list of active internal flights based on Datetime given as a parameter
        public async Task<IList<Flight>> GetFlightByDate(DateTime relative_to)
        {
            bool sync_all = Request.Query.ContainsKey("sync_all");
            List<Flight> flightslist = new List<Flight>();
            DateTime utcDate = relative_to.ToUniversalTime();


            if (_cache.TryGetValue("flightplans", out List<FlightPlan> flightplans))
            {
                //find all active internal flightplans with time bigger\equal to datetime given as a parameter
                List<FlightPlan> nonPlannedFlights = await Task.Run(() => flightplans.FindAll(g => 
                g.Initial_Location.Date_Time <= utcDate));
                nonPlannedFlights.ForEach(async flight =>
                {
                    //convert the flightplan's segment to list
                    var segments = flight.Segments.ToList();
                    //sum the total time of the journey of the plane through the segments
                    int totalTimeSpan = segments.Sum(v => v.Timespan_seconds);


                    if (flight.Initial_Location.Date_Time.AddSeconds(totalTimeSpan) > utcDate)
                    {

                        if (!sync_all)
                        {
                            //add internal flights
                            if (_cache.TryGetValue("flights", out Dictionary<string, Flight> allFlights))
                            {
                                // all flights are is_external false

                                /* if (!allFlights[flight.ID].Is_external) // local flights
                                 { */
                                flightslist.Add(allFlights[flight.ID]);
                                await Task.Run(() => LinearInterpolation(flight, utcDate));

                                // }
                            }
                        }
                        else
                        {

                            //add internal flights
                            if (_cache.TryGetValue("flights", out Dictionary<string, Flight> allFlights))
                            {
                                // all flights are is_external false

                                /* if (!allFlights[flight.ID].Is_external) // local flights
                                 { */
                                flightslist.Add(allFlights[flight.ID]);
                                await Task.Run(() => LinearInterpolation(flight, utcDate));

                                // }
                            }
                            //add external flights
                            if (_cache.TryGetValue("externalFlights", out List<Flight> externalFlights))
                            {
                                foreach (Flight _flight in externalFlights)
                                {
                                    flightslist.Add(_flight);
                                }
                                //  flightslist.Add(externalFlights[flight.ID]);
                                await Task.Run(() => LinearInterpolation(flight, utcDate));
                            }
                        }
                    }
                });

                return flightslist;
            }
            return null;
        }
    }
}