using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.core.DTos.Address
{
    public class CreateAddressDto
    {
        public string AddressLine { get; set; } = null!;

        public bool IsPrimary { get; set; }
    }
}
