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

        // Update the status of an order (e.g., processing, shipped, delivered)
        [HttpPatch("{id:length(24)}/status")]
        public async Task<ActionResult<AppResponse<Order>>> UpdateOrderStatus(string id, [FromBody] UpdateOrderStatusDto orderStatus)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var existingOrder = await _orderService.GetOrderByIdAsync(id);

            if (existingOrder == null)
                return NotFound(new AppResponse<Order> { Success = false, Message = "Order not found" });

            // Ensure order status is not updated after it has been delivered
            if (existingOrder.Status == OrderStatus.Shipped || existingOrder.Status == OrderStatus.Delivered)
                return BadRequest(new AppResponse<Order> { Success = false, Message = "Order cannot be updated after dispatch." });

            if (orderStatus.Status != OrderStatus.Delivered)
            {
                return BadRequest(new AppResponse<Order> { Success = false, Message = "You are not authorized to change this status." });
            }

            // If the order is being marked as Delivered, ensure all items are already delivered
            if (orderStatus.Status == OrderStatus.Delivered)
            {
                if (existingOrder.Items.Any(item => item.Status != OrderStatus.Delivered))
                {
                    return BadRequest(new AppResponse<Order> { Success = false, Message = "Cannot mark the order as Delivered because not all items are delivered." });
                }
            }

            await _orderService.UpdateOrderStatusAsync(id, orderStatus.Status, null);
            existingOrder.Status = orderStatus.Status;

            // TODO: it should inform to customer as a notification.

            return Ok(new AppResponse<Order> { Success = true, Data = existingOrder, Message = $"Order status updated to {orderStatus.Status}" });
        }

        // Update the status of an order items
        [HttpPatch("{orderId}/items/{productId}/status")]
        public async Task<ActionResult<AppResponse<Order>>> UpdateOrderItemStatus(string orderId, string productId, [FromBody] UpdateOrderItemStatusDto itemStatus)
        {
            var vendorId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return NotFound(new AppResponse<Order> { Success = false, Message = "Order not found" });
            }

            // Update the delivery status for items related to the vendor
            foreach (var item in order.Items.Where(i => i.VendorId == vendorId && i.ProductId == productId))
            {
                item.Status = itemStatus.Status;
            }

            // Check if all items are delivered
            if (order.Items.All(i => i.Status == OrderStatus.Delivered))
            {
                order.Status = OrderStatus.Delivered;
            }
            else if (order.Items.Any(i => i.Status == OrderStatus.Delivered))
            {
                order.Status = OrderStatus.PartiallyDelivered;
            }

            await _orderService.UpdateOrderAsync(order);

            return Ok(new AppResponse<Order>
            {
                Success = true,
                Data = order,
                Message = "Item delivery status updated successfully"
            });
        }

    }
}
