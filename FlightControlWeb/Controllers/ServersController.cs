using FlightControlWeb.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;

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
        [Route("servers")]
        [HttpPost("{Server}")]
        public IActionResult AddNewExternalServerToList(Server server)
        {
            List<Server> servers;
            if (!_cache.TryGetValue("servers", out servers))
            {

                servers = new List<Server>();
                servers.Add(server);
                _cache.Set("servers", servers);
            }
            else
            {
                servers.Add(server);
            }
            return Ok();
        }
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