using System;
using System.Collections.Generic;

namespace Ecommerce_Backend.Models;

public partial class Address
{
    public int Id { get; set; }

    public int UserId { get; set; } 

    public string AddressLine { get; set; } = null!;

    public bool IsPrimary { get; set; }

    public virtual User User { get; set; } = null!;
}
