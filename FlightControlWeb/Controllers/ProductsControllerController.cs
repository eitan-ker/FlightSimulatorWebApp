using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlightControlWeb.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlightControlWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsControllerController : ControllerBase
    {
        private IProductManager productsManager = new ProductsManager();

        // GET: api/ProductsController
        [HttpGet]
        public IEnumerable<Product> GetAllProducts()
        { 
            return productsManager.GetAllProducts();
        }

        // GET: api/ProductsController/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/ProductsController
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/ProductsController/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
