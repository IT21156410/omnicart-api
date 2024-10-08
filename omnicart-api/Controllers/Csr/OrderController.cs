﻿// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Prashantha K.G.M
// Student ID       : IT21169908
// Description      : Handle HTTP API requests related to csr order management. 
// Tutorial         : https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-8.0&tabs=visual-studio
// ***********************************************************************

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using omnicart_api.Models;
using omnicart_api.Requests;
using omnicart_api.Services;
using System.Security.Claims;

namespace omnicart_api.Controllers.Csr
{
    [Route("api/csr/orders")]
    [ApiController]
    [Authorize(Roles = "csr")]
    [ServiceFilter(typeof(ValidateModelAttribute))]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrderController(OrderService orderService)
        {
            _orderService = orderService;
        }

        // Get all orders
        [HttpGet]
        public async Task<ActionResult<AppResponse<List<Order>>>> Get()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(new AppResponse<List<Order>> { Success = true, Data = orders, Message = "Orders retrieved successfully" });
        }

        // Cancel an order (before it is dispatched)
        [HttpPatch("{id:length(24)}/cancel")]
        public async Task<ActionResult<AppResponse<Order>>> CancelOrder(string id, [FromBody] CancelOrderDto cancel)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var existingOrder = await _orderService.GetOrderByIdAsync(id);

            if (existingOrder == null)
            {
                return NotFound(new AppResponse<Order> { Success = false, Message = "Order not found" });
            }

            // Ensure the order is not already dispatched or delivered
            if (existingOrder.Status == OrderStatus.Shipped || existingOrder.Status == OrderStatus.Delivered)
            {
                return BadRequest(new AppResponse<Order> { Success = false, Message = "Order cannot be canceled after dispatch." });
            }

            // Update the order status to 'Cancelled' and add the cancellation note
            existingOrder.Status = OrderStatus.Cancelled;
            existingOrder.Note = cancel.Note;

            await _orderService.UpdateOrderStatusAsync(id, OrderStatus.Cancelled, cancel.Note);

            // TODO: it should inform to customer as a notification.

            return Ok(new AppResponse<Order> { Success = true, Data = existingOrder, Message = "Order canceled successfully" });
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

            await _orderService.UpdateOrderStatusAsync(id, orderStatus.Status, null);
            existingOrder.Status = orderStatus.Status;

            // TODO: it should inform to customer as a notification.

            return Ok(new AppResponse<Order> { Success = true, Data = existingOrder, Message = $"Order status updated to {orderStatus.Status}" });
        }

        // Get a specific order by its ID
        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<AppResponse<Order>>> GetOrderById(string id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null)
            {
                return NotFound(new AppResponse<Order> { Success = false, Message = "Order not found" });
            }

            return Ok(new AppResponse<Order> { Success = true, Data = order, Message = "Order retrieved successfully" });
        }

    }
}