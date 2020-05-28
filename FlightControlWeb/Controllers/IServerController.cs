using FlightControlWeb.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightControlWeb.Controllers
{
    public interface IServerController
    {
        public List<Server> GetServersList();
        public Task<IActionResult> AddNewExternalServerToList(Server server);
        public Task<IActionResult> DeleteExternalServerFromListByID(string ServerID);

    }
}
