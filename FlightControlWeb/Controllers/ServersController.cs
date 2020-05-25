using FlightControlWeb.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;


namespace FlightControlWeb.Controllers
{
    [Route("api")]
    [ApiController]
    public class ServersController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        public ServersController(IMemoryCache cache)
        {
            this._cache = cache;
        }

        // GET: /api/servers
        [Route("servers")]
        [HttpGet]
        public List<Server> GetServersList()
        {
            //if key with name "servers" doesnt exist in memcache, the create it and value will be list
            if (!_cache.TryGetValue("servers", out List<Server> servers))
            {
                return null;
            }
            else
            //else return the list of servers from memcache to client
            {
                return servers;
            }
        }
        // POST: /api/servers
        [Route("servers")]
        [HttpPost("{Server}")]
        public async Task<IActionResult> AddNewExternalServerToList(Server server)
        {
            List<Server> servers;
            try
            {
                //if key with name "servers" doesnt exist in memcache, the create it and value will be list
                if (!_cache.TryGetValue("servers", out servers))
                {
                    servers = new List<Server>
                    {
                        //add the server object given as a parameter to "servers" list 
                        server
                };
                    //create the "servers" list in memcache
                    _cache.Set("servers", servers);
                    //add flight and respective flightplan from the external server to the memcache DB
                    await Task.Run(() => importExternalFlights(server.ServerUrl.ToString(), server.ServerId));
                }
                else
                {
                    //handle case when client try to insert server and memcache DB exists and "servers" data structure exists in memcache
                    if (servers.Find(o => o.ServerId == server.ServerId) == null)
                    {
                        //add the server to "servers" list in memcache
                        servers.Add(server);
                        //add the flight from the server
                        await Task.Run(() => importExternalFlights(server.ServerUrl.ToString(), server.ServerId));
                    }
                    else
                    {
                        //handle case when the user try to insert server with existing ID in DB, then i wont let him to do that
                    }
                }
                return Ok();
            } 
            catch (Exception)
            {
                if (_cache.TryGetValue("servers", out servers))
                {
                    if (servers.Find(o => o.ServerId == server.ServerId) != null)
                    {
                        servers.Remove(server);
                    }
                }
                return BadRequest();
            }
        }
        private void importExternalFlights(string URL, string id)
        {
        //    string parsedURL = parseURL(URL);

            string request_str = URL + "/api/Flights?relative_to=";
            
            DateTime utcDate = DateTime.UtcNow.ToUniversalTime();
            string CurTime = parseTime(utcDate.ToString());
            request_str = request_str + CurTime;

            WebRequest request = WebRequest.Create(request_str);
            request.Method = "GET";
            HttpWebResponse response = null;
            response = (HttpWebResponse)request.GetResponse();
            string strResult = "";
            List<Flight> external_flights;
            using (Stream stream = response.GetResponseStream())
            {
                StreamReader sr = new StreamReader(stream);
                strResult = sr.ReadToEnd();
                external_flights = makeList(strResult);
                sr.Close();
            }

            // save in cache
            saveExternalFlights(external_flights);
            saveExternalFlightPlans(external_flights, URL);
            saveServerFlights(external_flights, id);
        }

        private void saveServerFlights(List<Flight> external_flights, string id)
        {
            Dictionary<string, List<Flight>> serverFlight;
            List<Flight> temp;
            if (!_cache.TryGetValue("server_flights", out serverFlight))
            {
                // serverFlight.Add(URL, external_flights);
                serverFlight = new Dictionary<string, List<Flight>>()
                {
                    { id, external_flights }
                };
                _cache.Set("server_flights", serverFlight);
            }
            else
            {
                // check if i have the URL then overWrite - add otherwise
                if (serverFlight.TryGetValue(id, out temp))
                {
                    if (temp.Count != 0) // already have the server
                    {
                        temp.Clear();
                        temp = external_flights;
                    }
                    else
                    {
                        temp = external_flights;
                    }
                }
                else
                {
                    serverFlight.Add(id, external_flights);
                }
            }
        }

        private void saveExternalFlightPlans(List<Flight> external_flights, string URL)
        {
            string request_str = URL + "/api/FlightPlan/";
            foreach (Flight flight in external_flights)
            {
                WebRequest request = WebRequest.Create(request_str + flight.FlightID);
                request.Method = "GET";
                HttpWebResponse response = null;
                response = (HttpWebResponse)request.GetResponse();
                string strResult = "";
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader sr = new StreamReader(stream);
                    strResult = sr.ReadToEnd();
                    saveExternalFlightPlans(strResult);
                    sr.Close();
                }
            }
        }
        private void saveExternalFlightPlans(string flightPlanStr)
        {
            List<FlightPlan> flightPlans;
            FlightPlan flightPlan = JsonConvert.DeserializeObject<FlightPlan>(flightPlanStr);
            if (!_cache.TryGetValue("flightPlan", out flightPlans))
            {
                flightPlans = new List<FlightPlan>();
                flightPlans.Add(flightPlan);
                _cache.Set("externalFlightPlans", flightPlans);
            } else
            {
                flightPlans.Add(flightPlan);
            }
        }
        private void saveExternalFlights(List<Flight> flights)
        {
            List<Flight> external_flights;
            if (!_cache.TryGetValue("externalFlights", out external_flights))
            {
                external_flights = flights;
                _cache.Set("externalFlights", flights);
            }
            else
            {
                foreach (Flight flight in flights)
                {
                    external_flights.Add(flight);
                }
            }
        }

        private string parseTime(string time)
        {
            string[] words = time.Split(' ');
            int count = 0;
            string parsedTime = "";
            foreach (var word in words)
            {
                if (count == 0)
                {
                    string[] date = word.Split('/');
                    parsedTime = date[2] + "-" + date[1] + "-" + date[0] + "T";
                }
                if (count == 1)
                {
                    parsedTime = parsedTime + word + "Z";
                    break;
                }
                count++;
            }
            return parsedTime;
        }

        private List<Flight> makeList(string json)
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

        // DELETE: /api/servers/{id}
        [Route("servers/{ServerID}")]
        [HttpDelete("{ServerID}")]
        // test post method with flightplan object from Postman
        public async Task<IActionResult> DeleteExternalServerFromListByID(string ServerID)
        {
            if (!_cache.TryGetValue("servers", out List<Server> servers))
            {
                return NotFound();
            }
            else
            {
                /*Server curserver = servers.Find(o => o.ServerId == ServerID);
                servers.Remove(curserver);*/
                foreach(var it in servers)
                {
                    if (string.Compare(it.ServerId, ServerID) == 0)
                    {
                        await Task.Run(() => deleteFlightsFromServer(ServerID));
                        servers.Remove(it);
                        return Ok();
                    }
                }
            }
            return NotFound();
        }
        private void deleteFlightsFromServer(string id)
        {
            List<Flight> serverFlights;
            Dictionary<string, List<Flight>> serverFlightsDic;   // dictionary
            //Dictionary<string, Flight> flights;
            List<FlightPlan> flightPlans;
            List<FlightPlan> externalFlights;
            if (_cache.TryGetValue("server_flights", out serverFlightsDic))
            {
                if (serverFlightsDic.TryGetValue(id, out serverFlights))
                {
                    if (_cache.TryGetValue("externalFlights", out externalFlights))
                    {
                        List<FlightPlan> temp = new List<FlightPlan>(externalFlights);
                        foreach (Flight server_flights in serverFlights)
                        {
                            foreach (FlightPlan external_flight in temp)
                            {
                                if (server_flights.FlightID.CompareTo(external_flight.ID) == 0)
                                {
                                    externalFlights.Remove(external_flight);
                                }
                            }
                        } // REMOVE FROM CACHE                        
                    } 
                    if (_cache.TryGetValue("flightplans", out flightPlans))
                    {
                        List<FlightPlan> temp = new List<FlightPlan>(flightPlans);
                        foreach (Flight server_flights in serverFlights)
                        {
                            foreach (FlightPlan flightPlan in temp)
                            {
                                if (server_flights.FlightID.CompareTo(flightPlan.ID) == 0)
                                {
                                    flightPlans.Remove(flightPlan);
                                }
                            }
                        }
                    }
                }
                serverFlightsDic.Remove(id);
            }
        }
    }
}