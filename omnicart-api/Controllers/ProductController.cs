using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using omnicart_api.Models;
using omnicart_api.Services;

namespace omnicart_api.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController(ProductService productService) : ControllerBase
    {
        private readonly ProductService _productService = productService;


        // Get all products or browse products by category
        [HttpGet]
        public async Task<ActionResult<AppResponse<List<Product>>>> GetAllProducts(string? category = null)
        {
            List<Product> products;


            if (!string.IsNullOrEmpty(category))
            {
                if (!ObjectId.TryParse(category, out _))
                {
                    return UnprocessableEntity(new AppResponse<List<Product>>
                    {
                        Success = false,
                        Message = "Invalid category id.",
                        ErrorCode = 422
                    });
                }

                // If category is provided, fetch products by category
                products = await _productService.GetProductByForeignIdAsync(category, foreignMatchProperty: "categoryId");
            }
            else
            {
                // If category is not provided, fetch all products
                products = await _productService.GetAllProductsAsync();
            }

            return Ok(new AppResponse<List<Product>> { Success = true, Data = products, Message = "Products retrieved successfully" });
        }

        // View detailed product information.
        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<AppResponse<Product>>> ViewProductById(string id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
                return NotFound(new AppResponse<Product> { Success = false, Message = "Product not found" });

            return Ok(new AppResponse<Product> { Success = true, Data = product, Message = "Product retrieved successfully" });
        }

        // Search products with filters
        [HttpGet("search")]
        public async Task<ActionResult<AppResponse<List<Product>>>> SearchProducts(
            string? name = null,
            string? category = null,
            double? minPrice = null,
            double? maxPrice = null,
            string? vendor = null,
            int? minRating = null,
            int? maxRating = null,
            string? sortBy = null,
            string? sortDirection = "asc")
        {
            if (!string.IsNullOrEmpty(vendor) && !ObjectId.TryParse(vendor, out _))
            {
                return UnprocessableEntity(new AppResponse<List<Product>>
                {
                    Success = false,
                    Message = "Invalid vendor id.",
                    ErrorCode = 422
                });
            }

            if (!string.IsNullOrEmpty(category) && !ObjectId.TryParse(category, out _))
            {
                return UnprocessableEntity(new AppResponse<List<Product>>
                {
                    Success = false,
                    Message = "Invalid category id.",
                    ErrorCode = 422
                });
            }

            // Validate price range
            if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
            {
                return UnprocessableEntity(new AppResponse<List<Product>>
                {
                    Success = false,
                    Message = "Invalid price range. 'minPrice' cannot be greater than 'maxPrice'.",
                    ErrorCode = 422
                });
            }

            // Validate rating range
            if (minRating.HasValue && maxRating.HasValue && (minRating < 0 || maxRating > 5 || minRating > maxRating))
            {
                return UnprocessableEntity(new AppResponse<List<Product>>
                {
                    Success = false,
                    Message = "Invalid rating range. Ratings must be between 0 and 5.",
                    ErrorCode = 422
                });
            }

            // Fetch products from the ProductService based on the filters
            var products = await _productService.SearchProductsAsync(
                name,
                category,
                minPrice,
                maxPrice,
                vendor,
                minRating,
                maxRating,
                sortBy,
                sortDirection!
            );

            // Return the response
            return Ok(new AppResponse<List<Product>>
            {
                Success = true,
                Data = products,
                Message = "Products retrieved successfully"
            });
        }
    }
}