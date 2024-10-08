// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Prashantha K.G.M
// Student ID       : IT21169908
// Description      : Handle HTTP API requests related to customer. 
// Tutorial         : https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-8.0&tabs=visual-studio
// ***********************************************************************

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using omnicart_api.Models;
using omnicart_api.Requests;
using omnicart_api.Services;
using System.Security.Claims;
using MongoDB.Driver;

namespace omnicart_api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    [Authorize(Roles = "customer")]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerService _customerService;
        private readonly UserService _userService;
        private readonly OrderService _orderService;

        public CustomerController(CustomerService customerService, UserService userService, OrderService orderService)
        {
            _customerService = customerService;
            _userService = userService;
            _orderService = orderService;
        }

        [HttpPost("cart/add")]
        public async Task<ActionResult> AddToCart([FromBody] CartItemDto cartItem)
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

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new AppResponse<string>
                {
                    Success = false,
                    Message = "User not found",
                    ErrorCode = 404
                });
            }

            // Check if the item already exists in the cart and update quantity
            var existingItem = user.Cart.FirstOrDefault(i => i.ProductId == cartItem.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += cartItem.Quantity;
            }
            else
            {
                user.Cart.Add(new CartItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice
                });
            }

            // Update the user's cart in the database
            await _customerService.UpdateUserCartAsync(userId, user.Cart);

            return Ok(new AppResponse<string>
            {
                Success = true,
                Message = "Product added to cart successfully."
            });
        }

        [HttpGet("cart")]
        public async Task<ActionResult<List<CartItem>>> GetCart()
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

            var user = await _customerService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new AppResponse<string>
                {
                    Success = false,
                    Message = "User not found",
                    ErrorCode = 404
                });
            }

            return Ok(new AppResponse<List<CartItem>>
            {
                Success = true,
                Data = user.Cart,
                Message = "Cart items retrieved successfully."
            });
        }

        [HttpPost("cart/purchase")]
        public async Task<ActionResult> Purchase([FromBody] PurchaseRequest purchaseRequest)
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

            // Retrieve the user's cart
            var user = await _customerService.GetUserByIdAsync(userId);
            if (user == null || user.Cart.Count == 0)
            {
                return BadRequest(new AppResponse<string>
                {
                    Success = false,
                    Message = "Cart is empty",
                    ErrorCode = 400
                });
            }

            var shippingAddress = !string.IsNullOrEmpty(purchaseRequest.ShippingAddress) ? purchaseRequest.ShippingAddress : user.ShippingAddress;

            if (string.IsNullOrEmpty(shippingAddress))
            {
                return BadRequest(new AppResponse<string>
                {
                    Success = false,
                    Message = "Shipping address is required",
                    ErrorCode = 400
                });
            }

            // Create a new Order from the cart
            var newOrder = new Order
            {
                UserId = user.Id,
                OrderNumber = _orderService.GenerateOrderNumber(),
                OrderDate = DateTime.UtcNow,
                Items = user.Cart.Select(cartItem => new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    VendorId = cartItem.VendorId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice,
                    Status = OrderStatus.Pending
                }).ToList(),
                Status = OrderStatus.Processing,
                PaymentStatus = PaymentStatus.Paid,
                TotalAmount = user.Cart.Sum(item => item.TotalPrice),
                ShippingAddress = shippingAddress,
            };

            await _orderService.CreateOrderAsync(newOrder);

            // Clear the user's cart after purchase
            user.Cart.Clear();
            await _customerService.UpdateUserCartAsync(user.Id!, user.Cart);

            return Ok(new AppResponse<string>
            {
                Success = true,
                Message = "Order placed successfully and marked as purchased."
            });
        }

    }
}
