using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace FlightControlWeb.Controllers
{
    [Route("api")]
    [ApiController]
    public class FlightsController : ControllerBase
    {
        private IMemoryCache _cache;
        public FlightsController(IMemoryCache cache)
        {
            this._cache = cache;
        }
        //DELETE: flightplan/{id}
        [Route("flights/{id}")]
        [HttpDelete("{id}")]
        public IActionResult DeleteFlightByID(string id)
        {
            return Ok();
        }
        [Route("flights")]
        [HttpGet("{Date}")]
        // test get method with Flight array from Postman
        public IActionResult GetFlightByDate(DateTime relative_to, bool sync_all = false)
        {
            if (sync_all)
            {

            }
            return Ok();
        }
    }
}