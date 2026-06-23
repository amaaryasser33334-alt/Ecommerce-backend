using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.core.DTos.Shipping;

public class CreateShippingDto
{
    public int OrderId { get; set; }
    public required string CarrierName { get; set; }
    public required string TrackingNumber { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
}