// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Fonseka M.M.N.H
// Student ID       : IT21156410
// Description      : Handle HTTP API requests related to admin product management. 
// ***********************************************************************

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using omnicart_api.Models;
using omnicart_api.Services;

namespace omnicart_api.Controllers.Admin
{
    [Route("api/admin/products")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductController(ProductService productService)
        {
            _productService = productService;
        }

        // Get all products
        [HttpGet]
        public async Task<ActionResult<AppResponse<List<Product>>>> Get()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized(new AppResponse<UserDto>
                {
                    Success = false,
                    Message = "User is not authenticated",
                    ErrorCode = 401
                });
            }

            var products = await _productService.GetAllProductsAsync();
            return Ok(new AppResponse<List<Product>> { Success = true, Data = products, Message = "Products retrieved successfully" });
        }

        // Activate/Deactivate a product
        [HttpPatch("{id:length(24)}/status")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<AppResponse<Product>>> SetProductStatus(string id, [FromBody] UpdateProductStatusDto status)
        {
           
            var existingProduct = await _productService.GetProductByIdAsync(id);

            if (existingProduct == null)
                return NotFound(new AppResponse<Product> { Success = false, Message = "Product not found" });
  
            await _productService.SetProductStatusAsync(id, status.Status);

            existingProduct.Status = status.Status;

            return Ok(new AppResponse<Product> { Success = true, Data = existingProduct, Message = $"Product status updated to {status.Status}" });
        }

        // Get a product by ID
        [HttpGet("{id:length(24)}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<AppResponse<Product>>> GetProductById(string id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (product == null)
                return NotFound(new AppResponse<Product> { Success = false, Message = "Product not found" });

            return Ok(new AppResponse<Product> { Success = true, Data = product, Message = "Product retrieved successfully" });
        }
    }
}