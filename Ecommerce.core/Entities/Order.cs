using System;
using System.Collections.Generic;

namespace Ecommerce_Backend.Models;

public partial class Order
{
    public int Id { get; set; }

    public int UserId { get; set; } 

    public DateTime OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public int Status { get; set; }

    public string? PaymentMethod { get; set; }

    public string? ShippingAddress { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Payment? Payment { get; set; }

    public virtual Shipping? Shipping { get; set; }

    public virtual User User { get; set; } = null!;
}
