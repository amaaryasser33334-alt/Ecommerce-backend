using System;
using System.Collections.Generic;

namespace Ecommerce_Backend.Models;

public partial class RefreshToken
{
    public int Id { get; set; }

    public int UserId { get; set; } 

    public string? Token { get; set; }

    public string? RefreshTokenHash { get; set; }

    public DateTime? RefreshTokenExpiresAt { get; set; }

    public DateTime? RefreshTokenRevokedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
