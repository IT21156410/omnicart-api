﻿// ***********************************************************************
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

namespace omnicart_api.Controllers
{
    [Route("api/auth/orders")]
    [ApiController]
    [Authorize(Roles = "customer")]
    [ServiceFilter(typeof(ValidateModelAttribute))]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrderController(OrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("history")]
        public async Task<ActionResult> GetOrderHistory()
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

            // Retrieve the order history for the customer
            var orderHistory = await _orderService.GetOrderHistoryByUserIdAsync(userId);
            if (orderHistory == null || !orderHistory.Any())
            {
                return NotFound(new AppResponse<string>
                {
                    Success = false,
                    Message = "No orders found",
                    ErrorCode = 404
                });
            }

            return Ok(new AppResponse<List<Order>>
            {
                Success = true,
                Message = "Order history retrieved successfully.",
                Data = orderHistory
            });
        }

        [HttpGet("track/{orderId}")]
        public async Task<ActionResult> TrackOrder(string orderId)
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

            // Retrieve the order details for the customer
            var order = await _orderService.GetUserOrderByIdAsync(userId, orderId);
            if (order == null)
            {
                return NotFound(new AppResponse<string>
                {
                    Success = false,
                    Message = "Order not found",
                    ErrorCode = 404
                });
            }

            return Ok(new AppResponse<Order>
            {
                Success = true,
                Message = "Order details retrieved successfully.",
                Data = order
            });
        }



        // Create a new order for the customer
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


    }
}
