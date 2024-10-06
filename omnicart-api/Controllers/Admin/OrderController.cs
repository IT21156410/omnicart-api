// ***********************************************************************
// APP NAME         : OmnicartAPI
// Author           : Prashantha K.G.M
// Student ID       : IT21169908
// Description      : Handle HTTP API requests related to admin order management. 
// Tutorial         : https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-8.0&tabs=visual-studio
// ***********************************************************************

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using omnicart_api.Models;
using omnicart_api.Requests;
using omnicart_api.Services;

namespace omnicart_api.Controllers.Admin
{
    [Route("api/admin/orders")]
    [ApiController]
    [Authorize(Roles = "admin")]
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

        // Get a specific order by its ID
        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<AppResponse<Order>>> GetOrderById(string id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null)
                return NotFound(new AppResponse<Order> { Success = false, Message = "Order not found" });

            return Ok(new AppResponse<Order> { Success = true, Data = order, Message = "Order retrieved successfully" });
        }

        // Update the payment status of an order
        [HttpPatch("{id:length(24)}/payment")]
        public async Task<ActionResult<AppResponse<Order>>> UpdatePaymentStatus(string id, [FromBody] UpdatePaymentStatusDto paymentStatus)
        {
            var existingOrder = await _orderService.GetOrderByIdAsync(id);

            if (existingOrder == null)
                return NotFound(new AppResponse<Order> { Success = false, Message = "Order not found" });

            await _orderService.UpdatePaymentStatusAsync(id, paymentStatus.PaymentStatus);
            existingOrder.PaymentStatus = paymentStatus.PaymentStatus;

            return Ok(new AppResponse<Order> { Success = true, Data = existingOrder, Message = $"Payment status updated to {paymentStatus.PaymentStatus}" });
        }

    }
}
