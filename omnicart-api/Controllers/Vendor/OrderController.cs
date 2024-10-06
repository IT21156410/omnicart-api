// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Prashantha K.G.M
// Student ID       : IT21169908
// Description      : Handle HTTP API requests related to vendor order management. 
// Tutorial         : https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-8.0&tabs=visual-studio
// ***********************************************************************

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using omnicart_api.Models;
using omnicart_api.Requests;
using omnicart_api.Services;
using System.Security.Claims;

namespace omnicart_api.Controllers.Vendor
{
    [Route("api/vendor/orders")]
    [ApiController]
    [Authorize(Roles = "vendor")]
    [ServiceFilter(typeof(ValidateModelAttribute))]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrderController(OrderService orderService)
        {
            _orderService = orderService;
        }

        // Get all orders for the vendor's products
        [HttpGet]
        public async Task<ActionResult<AppResponse<List<Order>>>> Get()
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

            var orders = await _orderService.GetOrdersByVendorIdAsync(userId);
            return Ok(new AppResponse<List<Order>> { Success = true, Data = orders, Message = "Orders retrieved successfully" });
        }

        // Get a specific order by its ID
        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<AppResponse<Order>>> GetOrderById(string id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null || order.UserId != userId)
            {
                return NotFound(new AppResponse<Order> { Success = false, Message = "Order not found" });
            }

            return Ok(new AppResponse<Order> { Success = true, Data = order, Message = "Order retrieved successfully" });
        }

        // Create a new order for the vendor
        [HttpPost]
        public async Task<ActionResult<AppResponse<Order>>> CreateOrder([FromBody] Order newOrder)
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

            newOrder.UserId = userId;

            await _orderService.CreateOrderAsync(newOrder);
            return Ok(new AppResponse<Order> { Success = true, Data = newOrder, Message = "Order created successfully" });
        }

        // Update the status of an order (e.g., processing, shipped, delivered)
        [HttpPatch("{id:length(24)}/status")]
        public async Task<ActionResult<AppResponse<Order>>> UpdateOrderStatus(string id, [FromBody] UpdateOrderStatusDto status)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var existingOrder = await _orderService.GetOrderByIdAsync(id);

            if (existingOrder == null || existingOrder.Items.Any(item => item.VendorId != userId))
                return NotFound(new AppResponse<Order> { Success = false, Message = "Order not found" });

            // Ensure order status is not updated after it has been delivered
            if (existingOrder.OrderStatus == OrderStatus.Shipped || existingOrder.OrderStatus == OrderStatus.Delivered)
                return BadRequest(new AppResponse<Order> { Success = false, Message = "Order cannot be updated after dispatch." });

            await _orderService.UpdateOrderStatusAsync(id, status.OrderStatus);
            existingOrder.OrderStatus = status.OrderStatus;

            return Ok(new AppResponse<Order> { Success = true, Data = existingOrder, Message = $"Order status updated to {status.OrderStatus}" });
        }

        // Cancel an order (before it is dispatched)
        [HttpPatch("{id:length(24)}/cancel")]
        public async Task<ActionResult<AppResponse<Order>>> CancelOrder(string id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var existingOrder = await _orderService.GetOrderByIdAsync(id);

            if (existingOrder == null || existingOrder.Items.Any(item => item.VendorId != userId))
                return NotFound(new AppResponse<Order> { Success = false, Message = "Order not found" });

            // Ensure the order is not already dispatched or delivered
            if (existingOrder.OrderStatus == OrderStatus.Shipped || existingOrder.OrderStatus == OrderStatus.Delivered)
                return BadRequest(new AppResponse<Order> { Success = false, Message = "Order cannot be canceled after dispatch." });

            await _orderService.UpdateOrderStatusAsync(id, OrderStatus.Cancelled);
            existingOrder.OrderStatus = OrderStatus.Cancelled;

            return Ok(new AppResponse<Order> { Success = true, Data = existingOrder, Message = "Order canceled successfully" });
        }

        // Delete an order (Vendor cannot typically delete an order, but in case of error, this can be useful)
        [HttpDelete("{id:length(24)}")]
        public async Task<ActionResult<AppResponse<Order>>> DeleteOrder(string id)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null || order.Items.Any(item => item.VendorId != userId))
                return NotFound(new AppResponse<Order> { Success = false, Message = "Order not found" });

            await _orderService.DeleteOrderAsync(id);
            return Ok(new AppResponse<Order> { Success = true, Data = order, Message = "Order deleted successfully" });
        }

    }
}
