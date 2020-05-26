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
using System.Text;
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
                    await Task.Run(() => _cache.Set("servers", servers));
                    //add flight and respective flightplan from the external server to the memcache DB
                 //   await Task.Run(() => importExternalFlights(server.ServerUrl.ToString(), server.ServerId));
                }
                else
                {
                    //handle case when client try to insert server and memcache DB exists and "servers" data structure exists in memcache
                    if (servers.Find(o => o.ServerId == server.ServerId) == null)
                    {
                        //add the server to "servers" list in memcache
                        await Task.Run(() => servers.Add(server));
                        //add the flight from the server
                   //     await Task.Run(() => importExternalFlights(server.ServerUrl.ToString(), server.ServerId));
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