using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.core.DTos.Review
{
    public class CreateReviewDto
    {
        public int ProductId { get; set; }

        public decimal Rating { get; set; }

        public string? Comment { get; set; }
    }
}
