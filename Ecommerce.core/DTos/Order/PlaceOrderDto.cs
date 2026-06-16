using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.core.DTos.Order
{
    public class PlaceOrderDto
    {
        public required string ShippingAddress { get; set; }
        public required string PaymentMethod { get; set; }
    }
}
