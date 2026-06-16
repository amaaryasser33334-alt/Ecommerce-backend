using System;
using System.Collections.Generic;

namespace Ecommerce_Backend.Models;

public partial class Wishlist
{
    public int Id { get; set; }

    public int UserId { get; set; } 

    public virtual User User { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
