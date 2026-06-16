using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.core.DTos.Review
{
    public class ReviewDTo
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string UserId { get; set; } = null!;

        public decimal Rating { get; set; }

        public string? Comment { get; set; }
    }
}
