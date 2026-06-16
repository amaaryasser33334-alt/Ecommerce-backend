using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.core.DTos.product
{
    public class ProductDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string? MainImageUrl { get; set; }
        public int CategoryId { get; set; }
        public required string CategoryName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
