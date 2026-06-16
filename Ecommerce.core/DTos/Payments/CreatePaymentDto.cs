using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.core.DTos.Payments
{
    public class CreatePaymentDto
    {
        public int OrderId { get; set; }
        public required string PaymentMethod { get; set; }

    }
}
