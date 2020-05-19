using FlightControlWeb.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

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
            if (!_cache.TryGetValue("servers", out servers))
            {
                servers = new List<Server>();
                servers.Add(server);
                string request = server.ServerUrl + "/api/Flights?relative_to=2020-12-26T23:58:21Z";
                string data = Get(request);
                _cache.Set("servers", servers);
            }
            else
            {
                servers.Add(server);
            }
            return Ok();
        }
        private string Get(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
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