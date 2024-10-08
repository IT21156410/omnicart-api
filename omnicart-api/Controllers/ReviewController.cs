// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Prashantha K.G.M
// Student ID       : IT21169908
// Description      : Handle HTTP API requests related to review management. 
// Tutorial         : https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-8.0&tabs=visual-studio
// ***********************************************************************

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using omnicart_api.Models;
using omnicart_api.Requests;
using omnicart_api.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace omnicart_api.Controllers
{
    [Route("api/review")]
    [ApiController]
    [ServiceFilter(typeof(ValidateModelAttribute))]
    public class ReviewController : ControllerBase
    {
        private readonly ReviewService _reviewService;

        public ReviewController(ReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// Adds a new review for a vendor.
        /// </summary>
        /// <param name="newReview">The review to create</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<AppResponse<Review>>> Post([FromBody] ReviewCreateDto newReview)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized(new AppResponse<string>
                {
                    Success = false,
                    Message = "User is not authenticated",
                    ErrorCode = 401
                });
            }

            var review = new Review
            {
                VendorId = newReview.VendorId,
                CustomerId = newReview.CustomerId,
                Comment = newReview.Comment,
                Rating = newReview.Rating,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _reviewService.CreateReviewAsync(review);
            await _reviewService.UpdateVendorAverageRatingAsync(newReview.VendorId);

            return CreatedAtAction(nameof(GetReviewsByVendorId), new { vendorId = newReview.VendorId }, new AppResponse<Review>
            {
                Success = true,
                Data = review,
                Message = "Review created successfully"
            });
        }

        /// <summary>
        /// Gets all reviews for a specific vendor.
        /// </summary>
        /// <param name="vendorId">The ID of the vendor</param>
        /// <returns>A list of reviews for the vendor</returns>
        [HttpGet("vendor/{vendorId}")]
        [Authorize(Roles = "vendor,admin")]
        public async Task<ActionResult<AppResponse<List<Review>>>> GetReviewsByVendorId(string vendorId)
        {
            var reviews = await _reviewService.GetReviewsByVendorIdAsync(vendorId);
            return Ok(new AppResponse<List<Review>>
            {
                Success = true,
                Data = reviews,
                Message = "Reviews retrieved successfully"
            });
        }

        /// <summary>
        /// Gets all reviews by a specific customer.
        /// </summary>
        /// <param name="customerId">The ID of the customer</param>
        /// <returns>A list of reviews by the customer</returns>
        [HttpGet("customer/{customerId}")]
        [Authorize(Roles = "customer")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<AppResponse<List<Review>>>> GetReviewsByCustomerId(string customerId)
        {
            var reviews = await _reviewService.GetReviewsByCustomerIdAsync(customerId);
            return Ok(new AppResponse<List<Review>>
            {
                Success = true,
                Data = reviews,
                Message = "Reviews retrieved successfully"
            });
        }

        /// <summary>
        /// Updates an existing comment by a customer.
        /// </summary>
        /// <param name="id">The ID of the review</param>
        /// <param name="updatedComment">The updated comment</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<AppResponse<Review>>> UpdateComment(string id, [FromBody] string updatedComment)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);

            if (review == null)
            {
                return NotFound(new AppResponse<Review>
                {
                    Success = false,
                    Message = "Review not found"
                });
            }

            // Validate if the customer is allowed to update the comment
            var currentCustomerId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (review.CustomerId != currentCustomerId)
            {
                return Unauthorized(new AppResponse<Review>
                {
                    Success = false,
                    Message = "You are not authorized to update this comment"
                });
            }

            // Allow only the comment to be updated
            review.Comment = updatedComment;
            review.UpdatedAt = DateTime.UtcNow;
            await _reviewService.UpdateReviewAsync(id, review);

            return Ok(new AppResponse<Review>
            {
                Success = true,
                Data = review,
                Message = "Comment updated successfully"
            });
        }

        /// <summary>
        /// Deletes a review by its ID.
        /// </summary>
        /// <param name="id">The ID of the review</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<AppResponse<string>>> DeleteReview(string id)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);

            if (review == null)
            {
                return NotFound(new AppResponse<string>
                {
                    Success = false,
                    Message = "Review not found"
                });
            }

            // Validate if the customer is allowed to update the comment
            var currentCustomerId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (review.CustomerId != currentCustomerId)
            {
                return Unauthorized(new AppResponse<Review>
                {
                    Success = false,
                    Message = "You are not authorized to delete this comment"
                });
            }

            await _reviewService.DeleteReviewAsync(id);

            return Ok(new AppResponse<string>
            {
                Success = true,
                Data = id,
                Message = "Review deleted successfully"
            });
        }
    }
}
