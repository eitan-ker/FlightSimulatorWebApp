using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlightControlWeb.Controllers
{
    [Route("api")]
    [ApiController]
    public class FlightPlanController : ControllerBase
    {
        [Route("flightplan")]
        [HttpPost]
        // test post method with flightplan object from Postman
        public async Task<IActionResult> FlightPlan(FlightPlan flight)
        {


            return Ok();
        }
    }
}