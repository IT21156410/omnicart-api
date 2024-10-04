// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Fonseka M.M.N.H
// Student ID       : IT21156410
// Description      : Handle HTTP API requests related to vendor product management. 
// ***********************************************************************

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using omnicart_api.Models;
using omnicart_api.Requests;
using omnicart_api.Services;

namespace omnicart_api.Controllers.Vendor
{
    [Route("api/vendor/products")]
    [ApiController]
    [ServiceFilter(typeof(ValidateModelAttribute))]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductController(ProductService productService)
        {
            _productService = productService;
        }

        // Get all products by vendor id
        [HttpGet]
        [Authorize(Roles = "vendor")]
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

            var products = await _productService.GetProductByUserIdAsync(userId);
            return Ok(new AppResponse<List<Product>> { Success = true, Data = products, Message = "Products retrieved successfully" });
        }

        // Create a new product
        [HttpPost]
        [Authorize(Roles = "vendor")]
        public async Task<ActionResult<AppResponse<Product>>> CreateProduct([FromBody] Product newProduct)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            // Validation
            if (string.IsNullOrWhiteSpace(newProduct.Name))
                ModelState.AddModelError(nameof(Product.Name), "Product name is required");
            if (string.IsNullOrWhiteSpace(newProduct.CategoryId))
                ModelState.AddModelError(nameof(Product.Category), "Product category is required");
            if (newProduct.Price <= 0)
                ModelState.AddModelError(nameof(Product.Price), "Product price must be greater than zero");
            if (newProduct.Stock < 0)
                ModelState.AddModelError(nameof(Product.Stock), "Stock cannot be negative");

            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(new AppResponse<Product>
                {
                    Success = false,
                    Message = "One or more validation errors occurred.",
                    Error = "Unprocessable Entity",
                    ErrorCode = 422,
                    ErrorData = UnprocessableEntity(ModelState)
                });
            }

            newProduct.UserId = userId;

            await _productService.CreateProductAsync(newProduct);
            return Ok(new AppResponse<Product> { Success = true, Data = newProduct, Message = "Product created successfully" });
        }

        // Get a product by ID
        [HttpGet("{id:length(24)}")]
        [Authorize(Roles = "vendor")]
        public async Task<ActionResult<AppResponse<Product>>> GetProductById(string id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (product == null || product.UserId != userId)
                return NotFound(new AppResponse<Product> { Success = false, Message = "Product not found" });

            return Ok(new AppResponse<Product> { Success = true, Data = product, Message = "Product retrieved successfully" });
        }

        // Update an existing product
        [HttpPut("{id:length(24)}")]
        [Authorize(Roles = "vendor")]
        public async Task<ActionResult<AppResponse<Product>>> UpdateProduct(string id, [FromBody] Product updatedProduct)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var existingProduct = await _productService.GetProductByIdAsync(id);

            if (existingProduct == null || existingProduct.UserId != userId)
                return NotFound(new AppResponse<Product> { Success = false, Message = "Product not found" });

            // Validation
            if (updatedProduct.Price <= 0)
                ModelState.AddModelError(nameof(Product.Price), "Product price must be greater than zero");
            if (updatedProduct.Stock < 0)
                ModelState.AddModelError(nameof(Product.Stock), "Stock cannot be negative");

            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(new AppResponse<Product>
                {
                    Success = false,
                    Message = "One or more validation errors occurred.",
                    Error = "Unprocessable Entity",
                    ErrorCode = 422,
                    ErrorData = UnprocessableEntity(ModelState)
                });
            }

            await _productService.UpdateProductAsync(id, updatedProduct);
            return Ok(new AppResponse<Product> { Success = true, Data = updatedProduct, Message = "Product updated successfully" });
        }

        // Delete a product by ID
        [HttpDelete("{id:length(24)}")]
        [Authorize(Roles = "vendor")]
        public async Task<ActionResult<AppResponse<Product>>> DeleteProduct(string id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var existingProduct = await _productService.GetProductByIdAsync(id);

            if (existingProduct == null || existingProduct.UserId != userId)
                return NotFound(new AppResponse<string> { Success = false, Message = "Product not found" });

            await _productService.DeleteProductAsync(id);
            return Ok(new AppResponse<Product> { Success = true, Data = existingProduct, Message = "Product deleted successfully" });
        }

        // Manage stock (add/remove stock)
        [HttpPatch("{id:length(24)}/stock")]
        [Authorize(Roles = "vendor")]
        public async Task<ActionResult<AppResponse<Product>>> UpdateStock(string id, [FromBody] UpdateProductStockDto newStock)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var existingProduct = await _productService.GetProductByIdAsync(id);

            if (existingProduct == null || existingProduct.UserId != userId)
                return NotFound(new AppResponse<Product> { Success = false, Message = "Product not found" });

            if (newStock.Stock < 0)
                ModelState.AddModelError(nameof(Product.Stock), "Stock cannot be negative");

            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(new AppResponse<Product>
                {
                    Success = false,
                    Message = "One or more validation errors occurred.",
                    Error = "Unprocessable Entity",
                    ErrorCode = 422,
                    ErrorData = UnprocessableEntity(ModelState)
                });
            }

            await _productService.UpdateStockAsync(id, newStock.Stock);

            existingProduct.Stock = newStock.Stock;
            return Ok(new AppResponse<Product> { Success = true, Data = existingProduct, Message = "Product stock updated successfully" });
        }
    }
}