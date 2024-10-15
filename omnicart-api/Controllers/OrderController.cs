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

namespace omnicart_api.Controllers
{
    [Route("api/auth/orders")]
    [ApiController]
    [Authorize(Roles = "customer")]
    [ServiceFilter(typeof(ValidateModelAttribute))]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly ProductService _productService;

        public OrderController(OrderService orderService, ProductService productService)
        {
            _orderService = orderService;
            _productService = productService;
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

        // Update an existing order before it's dispatched
        [HttpPut("{orderId}")]
        public async Task<ActionResult<AppResponse<Order>>> UpdateOrder(string orderId, [FromBody] UpdateOrderDto updatedOrder)
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

            var existingOrder = await _orderService.GetOrderByIdAsync(orderId);
            if (existingOrder == null)
            {
                return NotFound(new AppResponse<string>
                {
                    Success = false,
                    Message = "Order not found",
                    ErrorCode = 404
                });
            }

            // Ensure that only the owner of the order can update it
            if (existingOrder.UserId != userId)
            {
                return BadRequest(new AppResponse<UserDto>
                {
                    Success = false,
                    Message = "You are not allowed to update this order",
                    ErrorCode = 400
                });
            }

            // Check if the order has already been dispatched
            if (existingOrder.Status != OrderStatus.Pending && existingOrder.Status != OrderStatus.Processing)
            {
                return BadRequest(new AppResponse<string>
                {
                    Success = false,
                    Message = "Order cannot be updated because it has already been dispatched",
                    ErrorCode = 400
                });
            }

            // Update order details (before dispatch)
            //existingOrder.ShippingAddress = updatedOrder.ShippingAddress ?? existingOrder.ShippingAddress;
            //existingOrder.Items = updatedOrder.Items ?? existingOrder.Items;
            //existingOrder.Note = updatedOrder.Note ?? existingOrder.Note;

            // Update order details (before dispatch)
            existingOrder.ShippingAddress = updatedOrder.ShippingAddress ?? existingOrder.ShippingAddress;

            if (updatedOrder.Items != null)
            {
                // Revert stock for previously ordered items
                foreach (var existingOrderItem in existingOrder.Items)
                {
                    var product = await _productService.GetProductByIdAsync(existingOrderItem.ProductId);
                    if (product == null)
                    {
                        return NotFound(new AppResponse<string> { Success = false, Message = $"Product with ID {existingOrderItem.ProductId} not found", ErrorCode = 404 });
                    }

                    // Restore stock for removed or reduced items
                    var updatedOrderItem = updatedOrder.Items.FirstOrDefault(i => i.ProductId == existingOrderItem.ProductId);
                    if (updatedOrderItem == null || updatedOrderItem.Quantity < existingOrderItem.Quantity)
                    {
                        var quantityToRestore = existingOrderItem.Quantity - (updatedOrderItem?.Quantity ?? 0);
                        product.Stock += quantityToRestore;
                        await _productService.UpdateStockAsync(product, product.Stock);
                    }
                }


                existingOrder.Items = updatedOrder.Items.Select(itemDto =>
                {
                    var product = _productService.GetProductByIdAsync(itemDto.ProductId).Result; // Avoid async calls in a loop
                    if (product == null)
                    {
                        throw new Exception($"Product with ID {itemDto.ProductId} not found.");
                    }

                    // Check stock availability
                    if (itemDto.Quantity > product.Stock)
                    {
                        throw new Exception($"Not enough stock for product: {product.Name}. Available: {product.Stock}, Requested: {itemDto.Quantity}");
                    }

                    // Decrement stock for newly added or updated items
                    var existingItem = existingOrder.Items.FirstOrDefault(i => i.ProductId == itemDto.ProductId);
                    var quantityDifference = itemDto.Quantity - (existingItem?.Quantity ?? 0);
                    product.Stock -= quantityDifference;
                    _productService.UpdateStockAsync(product, product.Stock).Wait();

                    return new OrderItem
                    {
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = itemDto.UnitPrice,
                        Status = itemDto.Status
                    };
                }).ToList();
            }

            existingOrder.Note = updatedOrder.Note ?? existingOrder.Note;

            await _orderService.UpdateOrderAsync(existingOrder);

            return Ok(new AppResponse<Order>
            {
                Success = true,
                Data = existingOrder,
                Message = "Order updated successfully"
            });
        }

        // Order cancel request
        [HttpPost("{orderId}/cancel-request")]
        public async Task<ActionResult<AppResponse<string>>> RequestCancellation(string orderId, [FromBody] CancelRequestDto requestDto)
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

            var existingOrder = await _orderService.GetOrderByIdAsync(orderId);
            if (existingOrder == null || existingOrder.UserId != userId)
            {
                return NotFound(new AppResponse<string>
                {
                    Success = false,
                    Message = "Order not found",
                    ErrorCode = 404
                });
            }

            if (existingOrder.Status == OrderStatus.Cancelled)
            {
                return BadRequest(new AppResponse<string>
                {
                    Success = false,
                    Message = "Order is already cancelled",
                    ErrorCode = 400
                });
            }

            if (existingOrder.Status != OrderStatus.Processing && existingOrder.Status != OrderStatus.Pending)
            {
                return BadRequest(new AppResponse<string>
                {
                    Success = false,
                    Message = "Order cannot be cancelled because it is already dispatched",
                    ErrorCode = 400
                });
            }

            var cancellationRequest = new CancelRequest
            {
                OrderId = orderId,
                CustomerId = userId,
                Reason = requestDto.Reason,
                Status = CancelRequestStatus.Pending
            };

            await _orderService.CreateRequestAsync(cancellationRequest);

            // TODO: Notify CSR of the cancellation request

            return Ok(new AppResponse<string>
            {
                Success = true,
                Message = "Cancellation request submitted successfully. CSR will review it."
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

            // Decrement stock for each product in the order
            foreach (var orderItem in newOrder.Items)
            {
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                if (product == null)
                {
                    return NotFound(new AppResponse<string> { Success = false, Message = "Product not found", ErrorCode = 404 });
                }

                // Check if there is enough stock available
                if (product.Stock < orderItem.Quantity)
                {
                    return BadRequest(new AppResponse<string> { Success = false, Message = $"Not enough stock for product: {product.Name}", ErrorCode = 400 });
                }

                // Decrement the stock
                product.Stock -= orderItem.Quantity;
                await _productService.UpdateStockAsync(product!, product.Stock);
            }

            await _orderService.CreateOrderAsync(newOrder);
            return Ok(new AppResponse<Order> { Success = true, Data = newOrder, Message = "Order created successfully" });
        }
    }
}