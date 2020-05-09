using FlightControlWeb.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace FlightControlWeb.Controllers
{
    [Route("api")]
    [ApiController]
    public class ServersController : ControllerBase
    {
        private IMemoryCache _cache;
        public ServersController(IMemoryCache cache)
        {
            this._cache = cache;
        }
        [Route("servers")]
        [HttpGet]
        public IActionResult GetListOfExternalServers()
        {


            return Ok();
        }
        [Route("servers")]
        [HttpPost("{Server}")]
        public IActionResult AddNewExternalServerToList(Server server)
        {
            return Ok();
        }
        [Route("servers/{ServerID}")]
        [HttpDelete("{ServerID}")]
        // test post method with flightplan object from Postman
        public IActionResult DeleteExternalServerFromListByID(int ServerID)
        {


            return Ok();
        }
    }
}