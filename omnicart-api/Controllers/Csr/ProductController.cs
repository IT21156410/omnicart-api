// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Prashantha K.G.M
// Student ID       : IT21169908
// Description      : Handle HTTP API requests related to vendor product management. 
// ***********************************************************************

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using omnicart_api.Models;
using omnicart_api.Requests;
using omnicart_api.Services;

namespace omnicart_api.Controllers.Csr
{
    [Route("api/csr/products")]
    [ApiController]
    [ServiceFilter(typeof(ValidateModelAttribute))]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;
        private readonly OrderService _orderService;

        public ProductController(ProductService productService, OrderService orderService)
        {
            _productService = productService;
            _orderService = orderService;
        }

        // Get a product by ID
        [HttpGet("{id:length(24)}")]
        [Authorize(Roles = "csr")]
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