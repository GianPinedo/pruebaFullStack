using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.DTOs;
using OrderManagement.Application.Interfaces;

namespace OrderManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    /// <param name="createOrderDto">Order details</param>
    /// <returns>Created order</returns>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto createOrderDto)
    {
        var order = await _orderService.CreateAsync(createOrderDto);
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    /// <summary>
    /// Get an order by ID
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null)
            return NotFound(new ErrorResponse("Order not found"));

        return Ok(order);
    }

    /// <summary>
    /// Get orders with pagination and optional status filter
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="status">Optional status filter</param>
    /// <returns>Paginated list of orders</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null)
    {
        var query = new GetOrdersQuery
        {
            Page = page,
            PageSize = pageSize
        };

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<Domain.Enums.OrderStatus>(status, true, out var statusEnum))
        {
            query.Status = statusEnum;
        }

        var result = await _orderService.GetPagedAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// Cancel an order
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Updated order</returns>
    [HttpPut("{id:guid}/cancel")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> CancelOrder(Guid id)
    {
        var order = await _orderService.CancelAsync(id);
        return Ok(order);
    }
}

public record ErrorResponse(string Message, string? TraceId = null);