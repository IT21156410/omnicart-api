using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using omnicart_api.Models;
using omnicart_api.Services;
using System.Security.Claims;

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
    }
}
