using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.core.DTos.WishlistItem
{
    public class WishlistDto
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;

        public ICollection<WishlistItemDto> Products { get; set; }
            = new List<WishlistItemDto>();
    }
}
