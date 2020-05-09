using System.Collections.Generic;

namespace FlightControlWeb.Model
{
    interface IProductManager
    {
        IEnumerable<Product> GetAllProducts();
        Product GetProductById(int id);
        void AddProduct(Product p);
        void UpdateProduct(Product p);
        void deleteProduct(int id);
    }
}
