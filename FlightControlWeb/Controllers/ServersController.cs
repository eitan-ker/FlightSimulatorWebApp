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
            if (!_cache.TryGetValue("servers", out List<Server> servers))
            {
                return null;
            }
            else
            {
                return servers;
            }
        }
        // POST: /api/servers
        [Route("servers")]
        [HttpPost("{Server}")]
        public IActionResult AddNewExternalServerToList(Server server)
        {
            List<Server> servers;
            try
            {
                if (!_cache.TryGetValue("servers", out servers))
                {
                    servers = new List<Server>();
                    servers.Add(server);
                    _cache.Set("servers", servers);
                    importExternalFlights(server.ServerUrl.ToString());
                }
                else
                {
                    if (servers.Find(o => o.ServerId == server.ServerId) == null)
                    {
                        servers.Add(server);
                        importExternalFlights(server.ServerUrl.ToString());
                    }
                }
                return Ok();
            } 
            catch (Exception e)
            {
                throw e;
            }

        }
        private void importExternalFlights(string URL)
        {
            string parsedURL = parseURL(URL);
            string request_str = parsedURL + "/api/Flights?relative_to=";
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

        private string parseURL(string URL)
        {
            string parsedURL = "";
            string[] words = URL.Split('/');
            int counter = 0;
            foreach (var word in words)
            {
                if (word.Equals(""))
                {
                    parsedURL = parsedURL + "//";
                } else
                {
                    parsedURL = parsedURL + word;
                }
                counter++;
                if (counter == 3)
                {
                    break;
                }
            }
            return parsedURL;
        }

        private List<Flight> makeList(string json)
        {
            try
            {
                List<Flight> all_flights = new List<Flight>();
                JArray json_convert = JsonConvert.DeserializeObject<JArray>(json);
                // throws exception when server has no flights - stops program
                foreach (var elem in json_convert.Children())
                {
                    Flight external_flight = new Flight();
                    foreach (JProperty flight in elem.Children())
                    {
                        external_flight.setFlight(flight.Name.ToString(), flight.First.ToString());
                    }
                    all_flights.Add(external_flight);
                }
                return all_flights;
            } 
            catch (Exception e)
            {
                throw e;
            }
            
        } 

        // DELETE: /api/servers/{id}
        [Route("servers/{ServerID}")]
        [HttpDelete("{ServerID}")]
        // test post method with flightplan object from Postman
        public IActionResult DeleteExternalServerFromListByID(string ServerID)
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
                        servers.Remove(it);
                        return Ok();
                    }
                }
            }
            return NotFound();
        }
    }
}