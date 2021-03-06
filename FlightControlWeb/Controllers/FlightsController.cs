﻿using FlightControlWeb.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

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
                List<FlightPlan> tempPlan = new List<FlightPlan>(flightplans);
                /*loop throgh flight plans in datasctruture and find the requested flightplan, if it exist then delete it from the flightplans 
                 * data structure, remove both flightplan and respective flight*/
                foreach (var it in flightplans)
                {
                    //remove b
                    if (string.Compare(it.ID, id) == 0)
                    {
                        await Task.Run(() => tempPlan.Remove(it));
                        if(_cache.TryGetValue("flights", out Dictionary<string, Flight> flights))
                        {
                            await Task.Run(() => flights.Remove(id));
                        }
                        if (_cache.TryGetValue("externalFlights", out List<Flight> ex_flights))
                        {
                            List<Flight> temp = new List<Flight>(ex_flights);
                            foreach (Flight _flight in ex_flights)
                            {    
                                if (it.ID.CompareTo(_flight.FlightID) == 0)
                                {
                                    temp.Remove(_flight);
                                }
                            }
                            ex_flights = temp;
                        }
                        return Ok();
                    }
                }
                flightplans = tempPlan;
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
                        if (flights != null)
                        {
                            if (flights.ContainsKey(flight.ID))
                            {
                                flights[flight.ID].Latitude = cameFromLati + (relative * (element.Latitude - cameFromLati));
                                flights[flight.ID].Longitude = cameFromLong + (relative * (element.Longitude - cameFromLong));
                            }
                        }
                        var ex_flights = (List<Flight>)_cache.Get("externalFlights");
                        if (ex_flights != null)
                        {
                            foreach (Flight _flight in ex_flights)
                            {
                                if (_flight.FlightID.CompareTo(flight.ID) == 0)
                                {
                                    _flight.Latitude = cameFromLati + (relative * (element.Latitude - cameFromLati));
                                    _flight.Longitude = cameFromLong + (relative * (element.Longitude - cameFromLong));
                                }
                            }
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

            await Task.Run(() => ImportExternalFlights());
            if (_cache.TryGetValue("flightplans", out List<FlightPlan> flightplans))
            {        
                //find all active internal flightplans with time bigger\equal to datetime given as a parameter
                List<FlightPlan> nonPlannedFlights = flightplans.FindAll(g => 
                g.Initial_Location.Date_Time <= utcDate);
                await Task.Run(() => nonPlannedFlights.ForEach(flight =>
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
                                LinearInterpolation(flight, utcDate);

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
                                if (allFlights.TryGetValue(flight.ID, out Flight _flight))
                                {
                                    flightslist.Add(_flight);
                                    LinearInterpolation(flight, utcDate);
                                }


                                // }
                            }
                            //add external flights
                            if (_cache.TryGetValue("externalFlights", out List<Flight> externalFlights))
                            {
                                foreach (Flight _flight in externalFlights)
                                {
                                    if (_flight.FlightID.CompareTo(flight.ID) == 0)
                                    {
                                        flightslist.Add(_flight);
                                    }
                                }
                                //  flightslist.Add(externalFlights[flight.ID]);
                                LinearInterpolation(flight, utcDate);
                            }
                        }
                    }

                }));

                return flightslist;
            }
            return null;
        }
        private void ImportExternalFlights()
        {
            _cache.TryGetValue("servers", out List<Server> allServers);
            if (allServers != null)
            {
                try
                {
                    foreach (Server server in allServers)
                    {

                        string request_str = server.ServerUrl + "/api/Flights?relative_to=";
                        DateTime utcDate = DateTime.UtcNow.ToUniversalTime();
                        string CurTime = ParseTime(utcDate);
                        request_str = request_str + CurTime + "&sync_all";
                        WebRequest request = WebRequest.Create(request_str);
                        request.Method = "GET";
                        HttpWebResponse response = null;
                        response = (HttpWebResponse)request.GetResponse();
                        string strResult = "";
                        List<Flight> external_flights;
                        try
                        {
                            using (Stream stream = response.GetResponseStream())
                            {
                                StreamReader sr = new StreamReader(stream);
                                strResult = sr.ReadToEnd();
                                external_flights = MakeList(strResult);
                                sr.Close();
                            }
                            // save in cache
                            if (external_flights != null)
                            {
                                SaveExternalFlights(external_flights);
                                FindExternalFlightPlans(external_flights, server.ServerUrl);
                                SaveServerFlights(external_flights, server.ServerId);
                            }
                        } catch
                        {
                            Console.WriteLine("problem in reading da");

                        }

                    }
                }
                catch
                {
                    Console.WriteLine("some problem accoured during run");
                }
                    
            }
        }
        private void SaveServerFlights(List<Flight> external_flights, string ServerId)
        {
            List<Flight> tempFlights = new List<Flight>(external_flights);
            if (!_cache.TryGetValue("server_flights", out Dictionary<string, List<Flight>> serverFlight))
            {
                // serverFlight.Add(URL, external_flights);
                serverFlight = new Dictionary<string, List<Flight>>()
                {
                    { ServerId, tempFlights }
                };
                _cache.Set("server_flights", serverFlight);
            }
            else
            {
                // check if i have the URL then overWrite - add otherwise
                if (serverFlight.TryGetValue(ServerId, out List<Flight> temp))
                {
                    if (temp.Count != 0) // already have the server
                    {
                        temp.Clear();
                    }
                    foreach(Flight _flight in tempFlights)
                    {
                        temp.Add(_flight);
                    }
                }
                else
                {
                    serverFlight.Add(ServerId, tempFlights);
                }
            }
        }
        private void FindExternalFlightPlans(List<Flight> external_flights, string URL)
        {
            string request_str = URL + "/api/FlightPlan/";
            foreach (Flight flight in external_flights)
            {
                string _request = request_str + flight.FlightID;
                WebRequest request = WebRequest.Create(_request);
                request.Method = "GET";
                HttpWebResponse response = null;
                response = (HttpWebResponse)request.GetResponse();
                string strResult = "";
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader sr = new StreamReader(stream);
                    try
                    {
                        strResult = sr.ReadToEnd();
                        SaveExternalFlightPlans(strResult, flight.FlightID); // if flight plan doesn;t have the id
                        sr.Close();
                    } catch
                    {
                        Console.WriteLine("problem readnig from server");
                    }
                    
                }
            }
        }
        private void SaveExternalFlightPlans(string flightPlanStr, string id)
        {
            try
            {
                FlightPlan flightPlan = JsonConvert.DeserializeObject<FlightPlan>(flightPlanStr);
                if (flightPlan.Company_Name != null)
                {
                    flightPlan.ID = id;
                    if (!_cache.TryGetValue("flightplans", out List<FlightPlan> flightPlans))
                    {
                        flightPlans = new List<FlightPlan>();
                        flightPlans.Add(flightPlan);
                        _cache.Set("flightplans", flightPlans);
                    }
                    else
                    {
                        int count = 0;
                        foreach (FlightPlan plan in flightPlans) // if new flight id is different from all flights
                        {
                            if (flightPlan.ID.CompareTo(plan.ID) != 0)
                            {
                                count++;
                            }
                        }
                        if (count == flightPlans.Count)
                        {
                            flightPlans.Add(flightPlan);

                        }
                    }
                }
            } catch
            {
                Console.WriteLine("problem in saving external flight plan");

            }
        }
        private void SaveExternalFlights(List<Flight> flights)
        {
            if (!_cache.TryGetValue("externalFlights", out List < Flight > external_flights))
            {
                _cache.Set("externalFlights", flights);
            }
            else
            {
                if (external_flights.Count == 0)
                {
                    foreach (Flight flight in flights)
                    {
                        external_flights.Add(flight);
                    }

                } else
                {
                    foreach (Flight flight in flights)
                    {
                        int count = 0;
                        foreach (Flight ext_flight in external_flights) // if new flight id is different from all flights
                        {
                            if (ext_flight.FlightID.CompareTo(flight.FlightID) != 0)
                            {
                                count++;
                            }
                        }
                        if (count == external_flights.Count)
                        {
                            external_flights.Add(flight);
                        }
                    }
                }
                
            }
        }
        private string ParseTime(DateTime time)
        {
            //string[] words = time.Split(' ');
            //int count = 0;
            StringBuilder parsedTime = new StringBuilder(50);
            int monthNum = time.Month;
            int dayNum = time.Day;
            int yearNum = time.Year;
            int hoursNum = time.Hour;
            int minutesNum = time.Minute;
            int secondsNum = time.Second;
            string month = ParseTString(monthNum);
            string day = ParseTString(dayNum);
            string year = ParseTString(yearNum);
            string hour = ParseTString(hoursNum);
            string minutes = ParseTString(minutesNum);
            string seconds = ParseTString(secondsNum);
            parsedTime.Append(year).Append("-").Append(month).Append("-").Append(day).Append("T").Append(hour).Append(":").Append(minutes).Append(":").Append(seconds).Append("Z");
            string finalParsedtime = parsedTime.ToString();

            return finalParsedtime;
        }
        private string ParseTString(int val)
        {
            return (val < 10) ? "0" + val : val.ToString();
        }
        private List<Flight> MakeList(string json)
        {
            try
            {
                List<Flight> all_flights = new List<Flight>();
                JArray json_convert = JsonConvert.DeserializeObject<JArray>(json);
                // throws exception when server has no flights - stops program
                if (json_convert != null) // if not empty
                {
                    foreach (var elem in json_convert.Children())
                    {
                        Flight external_flight = new Flight();
                        foreach (JProperty flight in elem.Children())
                        {
                            external_flight.SetFlight(flight.Name.ToString(), flight.First.ToString());
                        }
                        all_flights.Add(external_flight);
                    }
                }
                return all_flights;
            }
            catch (Exception)
            {
                return null;
            }

        }
    }
}