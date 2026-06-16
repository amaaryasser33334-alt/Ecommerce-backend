using System;
using System.Collections.Generic;

namespace Ecommerce_Backend.Models;

public partial class Payment
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public decimal? Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public int PaymentStatus { get; set; }

    public DateTime TransactionDate { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}
