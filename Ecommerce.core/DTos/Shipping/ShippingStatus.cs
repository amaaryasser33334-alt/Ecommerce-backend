using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.core.Enums;

public enum ShippingStatus
{
    Preparing = 0,
    Shipped = 1,
    OutForDelivery = 2,
    Delivered = 3,
    Failed = 4
}