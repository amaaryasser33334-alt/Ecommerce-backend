using System;
using System.Collections.Generic;

namespace Ecommerce_Backend.Models;

public partial class Shipping
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string? CarrierName { get; set; }

    public string? TrackingNumber { get; set; }

    public int ShippingStatus { get; set; }

    public DateTime? EstimatedDeliveryDate { get; set; }

    public DateTime? ActualDeliveryDate { get; set; }

    public virtual Order Order { get; set; } = null!;
}
