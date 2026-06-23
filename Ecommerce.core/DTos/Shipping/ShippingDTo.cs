using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ecommerce.core.Enums;

namespace Ecommerce.core.DTos.Shipping;

public class ShippingDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string? CarrierName { get; set; }
    public string? TrackingNumber { get; set; }
    public ShippingStatus ShippingStatus { get; set; }
    public required string ShippingStatusName { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
}