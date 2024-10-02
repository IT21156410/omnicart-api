// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Fonseka M.M.N.H
// Student ID       : IT21156410
// Description      : Handle HTTP API requests related to product management. 
// ***********************************************************************

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using omnicart_api.Models;
using omnicart_api.Requests;
using omnicart_api.Services;

namespace omnicart_api.Controllers
{
    [Route("api/products")]
    [ApiController]
    [ServiceFilter(typeof(ValidateModelAttribute))]
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
            // TODO: Authorization Check 
            // if (!User.IsInRole("admin"))
            //     return Forbid("You do not have permission to view all products");
            var products = await _productService.GetAllProductsAsync();
            return Ok(new AppResponse<List<Product>> { Success = true, Data = products, Message = "Products retrieved successfully" });
        }

        // Create a new product
        [HttpPost]
        public async Task<ActionResult<AppResponse<Product>>> CreateProduct([FromBody] Product newProduct)
        {
            // TODO: Authorization Check
            // if (!User.IsInRole("vendor"))
            //     return Forbid(new AppResponse<Product> { Success = false, Message = "You do not have permission to create products" });

            // Validation
            if (string.IsNullOrWhiteSpace(newProduct.Name))
                ModelState.AddModelError(nameof(Product.Name), "Product name is required");
            if (string.IsNullOrWhiteSpace(newProduct.Category))
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

            await _productService.CreateProductAsync(newProduct);
            return Ok(new AppResponse<Product> { Success = true, Data = newProduct, Message = "Product created successfully" });
        }

        // Get a product by ID
        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<AppResponse<Product>>> GetProductById(string id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
                return NotFound(new AppResponse<Product> { Success = false, Message = "Product not found" });

            return Ok(new AppResponse<Product> { Success = true, Data = product, Message = "Product retrieved successfully" });
        }

        // Update an existing product
        [HttpPut("{id:length(24)}")]
        public async Task<ActionResult<AppResponse<Product>>> UpdateProduct(string id, [FromBody] Product updatedProduct)
        {
            // TODO: Authorization Check
            // if (!User.IsInRole("vendor") && !User.IsInRole("admin"))
            //     return Forbid(new AppResponse<Product> { Success = false, Message = "You do not have permission to update this product" });

            var existingProduct = await _productService.GetProductByIdAsync(id);

            if (existingProduct == null)
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
        public async Task<ActionResult<AppResponse<Product>>> DeleteProduct(string id)
        {
            // TODO: Authorization Check
            // if (!User.IsInRole("vendor") && !User.IsInRole("admin"))
            //     return Forbid(new AppResponse<string> { Success = false, Message = "You do not have permission to delete this product" });

            var existingProduct = await _productService.GetProductByIdAsync(id);

            if (existingProduct == null)
                return NotFound(new AppResponse<string> { Success = false, Message = "Product not found" });

            await _productService.DeleteProductAsync(id);
            return Ok(new AppResponse<Product> { Success = true, Data = existingProduct, Message = "Product deleted successfully" });
        }

        // Activate/Deactivate a product
        [HttpPatch("{id:length(24)}/status")]
        public async Task<ActionResult<AppResponse<Product>>> SetProductStatus(string id, [FromBody] UpdateProductStatusDto status)
        {
            // TODO: Authorization Check
            // if (!User.IsInRole("vendor") && !User.IsInRole("admin"))
            //     return Forbid(new AppResponse<Product> { Success = false, Message = "You do not have permission to change the status of this product" });

            var existingProduct = await _productService.GetProductByIdAsync(id);

            if (existingProduct == null)
                return NotFound(new AppResponse<Product> { Success = false, Message = "Product not found" });

            await _productService.SetProductStatusAsync(id, status.Status);

            existingProduct.Status = status.Status;

            return Ok(new AppResponse<Product> { Success = true, Data = existingProduct, Message = $"Product status updated to {status.Status}" });
        }

        // Manage stock (add/remove stock)
        [HttpPatch("{id:length(24)}/stock")]
        public async Task<ActionResult<AppResponse<Product>>> UpdateStock(string id, [FromBody] UpdateProductStockDto newStock)
        {
            // TODO: Authorization Check
            // if (!User.IsInRole("vendor"))
            //     return Forbid(new AppResponse<Product> { Success = false, Message = "You do not have permission to change the status of this product" });

            var existingProduct = await _productService.GetProductByIdAsync(id);

            if (existingProduct == null)
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
