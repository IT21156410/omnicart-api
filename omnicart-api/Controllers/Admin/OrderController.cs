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
using System.Security.Claims;

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

            await _orderService.UpdateOrderStatusAsync(existingOrder, OrderStatus.Cancelled, cancel.Note);

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

            await _orderService.UpdateOrderStatusAsync(existingOrder, orderStatus.Status, null);
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

        // Update the status of an order items
        [HttpPatch("{orderId}/items/{productId}/status")]
        public async Task<ActionResult<AppResponse<Order>>> UpdateOrderItemStatus(string orderId, string productId, [FromBody] UpdateOrderItemStatusDto itemStatus)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return NotFound(new AppResponse<Order> { Success = false, Message = "Order not found" });
            }

            // Update the delivery status for items
            foreach (var item in order.Items.Where(i => i.ProductId == productId))
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

        // Delete an order (cannot typically delete an order, but in case of error, this can be useful)
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

        // Order cancel request process
        [HttpPost("cancel/{requestId}/process")]
        public async Task<ActionResult<AppResponse<string>>> ProcessCancellationRequest(string requestId, [FromBody] ProcessCancelDto processDto)
        {
            var cancellationRequest = await _orderService.GetRequestByIdAsync(requestId);
            if (cancellationRequest == null)
            {
                return NotFound(new AppResponse<string>
                {
                    Success = false,
                    Message = "Cancellation request not found",
                    ErrorCode = 404
                });
            }

            if (cancellationRequest.Status != CancelRequestStatus.Pending)
            {
                return BadRequest(new AppResponse<string>
                {
                    Success = false,
                    Message = "Cancellation request is already processed",
                    ErrorCode = 400
                });
            }

            cancellationRequest.Status = processDto.IsApproved ? CancelRequestStatus.Approved : CancelRequestStatus.Rejected;

            await _orderService.UpdateRequestAsync(cancellationRequest);

            if (processDto.IsApproved)
            {
                //var order = await _orderService.GetOrderByIdAsync(cancellationRequest.OrderId);
                //order.Status = OrderStatus.Cancelled;
                //await _orderService.UpdateOrderStatusAsync(order.Id, OrderStatus.Cancelled, "Order cancelled by CSR.");

                // TODO: notify customer about the cancellation
            }

            return Ok(new AppResponse<string>
            {
                Success = true,
                Message = processDto.IsApproved ? "Cancellation request approved." : "Cancellation request rejected."
            });
        }

        // Get all cancel requests
        [HttpGet("cancel-requests")]
        public async Task<ActionResult<AppResponse<List<CancelRequest>>>> GetCancellationRequests()
        {
            var requests = await _orderService.GetAllCancellationRequestsAsync();

            return Ok(new AppResponse<List<CancelRequest>>
            {
                Success = true,
                Data = requests,
                Message = "Cancellation requests retrieved successfully."
            });
        }



    }
}
