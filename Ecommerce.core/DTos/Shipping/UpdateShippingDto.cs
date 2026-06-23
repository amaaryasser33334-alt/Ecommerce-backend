using Ecommerce.core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.core.DTos.Shipping;

public class UpdateShippingDto
{
    public ShippingStatus ShippingStatus { get; set; }
    public string? TrackingNumber { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
}
