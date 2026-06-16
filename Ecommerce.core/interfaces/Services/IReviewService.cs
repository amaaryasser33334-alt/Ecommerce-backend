using Ecommerce.core.DTos.Review;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.core.interfaces.Services
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewDTo>> GetProductReviewsAsync(
            int productId);

        Task AddReviewAsync(
            string userId,
            CreateReviewDto dto);

        Task DeleteReviewAsync(
            int reviewId);
    }
}
