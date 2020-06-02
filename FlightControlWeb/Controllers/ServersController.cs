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
    public class ServersController : ControllerBase, IServerController
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
                }
                else
                {
                    await Task.Run(() => AddNewExternalServerToListTryElse(servers, server));
                }
                return Ok();
            }
            catch (Exception)
            {
                if (_cache.TryGetValue("servers", out servers))
                {
                    AddNewExternalServerToListCatchIf(servers, server);
                }
                return BadRequest();
            }
        }

        private void AddNewExternalServerToListTryElse(List<Server> servers, Server server)
        {
            //handle case when client try to insert server and memcache DB exists and "servers" data structure exists in memcache
            if (servers.Find(o => o.ServerId == server.ServerId) == null)
            {
                //add the server to "servers" list in memcache
                servers.Add(server);
                //add the flight from the server
                //     await Task.Run(() => importExternalFlights(server.ServerUrl.ToString(), server.ServerId));
            }
            else
            {
                //handle case when the user try to insert server with existing ID in DB, then i wont let him to do that
            }
        }

        private void AddNewExternalServerToListCatchIf(List<Server> servers, Server server)
        {
            if (servers.Find(o => o.ServerId == server.ServerId) != null)
            {
                servers.Remove(server);
            }
        }

        // DELETE: /api/servers/{id}
        [Route("servers/{ServerID}")]
        [HttpDelete("{ServerID}")]
        // test post method with flightplan object from Postman
        public async Task<IActionResult> DeleteExternalServerFromListByID(string ServerID)
        {
            _cache.TryGetValue("servers", out List<Server> servers);
            if (servers == null)
            {
                return NotFound();
            }

            /*Server curserver = servers.Find(o => o.ServerId == ServerID);
            servers.Remove(curserver);*/
            foreach (var it in servers)
            {
                if (string.Compare(it.ServerId, ServerID) == 0)
                {
                    await Task.Run(() => DeleteFlightsFromServer(ServerID));
                    servers.Remove(it);
                    return Ok();
                }
            }

            return NotFound();
        }

        private void DeleteFlightsFromServer(string id)
        {
            if (_cache.TryGetValue("server_flights", out Dictionary<string,
                List<Flight>> serverFlightsDic))
            {
                if (serverFlightsDic.TryGetValue(id, out List<Flight> serverFlights))
                {
                    DeleteFlightsFromServerIfIf(serverFlights);
                }
                serverFlightsDic.Remove(id);
            }
        }

        private void DeleteFlightsFromServerIfIf(List<Flight> serverFlights)
        {
            if (_cache.TryGetValue("externalFlights", out List<Flight> externalFlights))
            {
                List<Flight> temp = new List<Flight>(externalFlights);
                foreach (Flight server_flights in serverFlights)
                {
                    DeleteFlightsFromServerIfIfExFlightsLoop(server_flights, externalFlights,
                        temp);
                } // REMOVE FROM CACHE                        
            }
            if (_cache.TryGetValue("flightplans", out List<FlightPlan> flightPlans))
            {
                List<FlightPlan> temp = new List<FlightPlan>(flightPlans);
                foreach (Flight server_flights in serverFlights)
                {
                    DeleteFlightsFromServerIfIfFlightPlansLoop(server_flights, flightPlans,
                        temp);
                }
            }
        }

        private void DeleteFlightsFromServerIfIfExFlightsLoop(Flight server_flights,
            List<Flight> externalFlights, List<Flight> temp)
        {
            foreach (Flight external_flight in temp)
            {
                if (server_flights.FlightID.CompareTo(external_flight.FlightID) == 0)
                {
                    externalFlights.Remove(external_flight);
                }
            }
        }

        private void DeleteFlightsFromServerIfIfFlightPlansLoop(Flight server_flights,
            List<FlightPlan> flightPlans, List<FlightPlan> temp)
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