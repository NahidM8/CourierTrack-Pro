using System.Security.Claims;

namespace CourierTrack.API.Controllers;

[Authorize]
[Route("api/v1/orders")]
[ApiController]
public class OrdersController(
    IOrderService orderService,
    IValidator<CreateOrderDto> createOrderValidator,
    IValidator<UpdateOrderDto> updateOrderValidator
    ) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll()
    {
        var orders = await orderService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<OrderDto>>.SuccessResult(orders));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id)
    {
        var order = await orderService.GetByIdAsync(id);
        return Ok(ApiResponse<OrderDto>.SuccessResult(order));
    }

    [AllowAnonymous]
    [HttpGet("track/{trackingNo}")]
    public async Task<ActionResult<OrderDto>> GetByTrackingNumber(string trackingNumber)
    {
        var order = await orderService.GetByTrackingNumberAsync(trackingNumber);
        return Ok(ApiResponse<OrderDto>.SuccessResult(order));
    }

    [HttpGet("customer/{customerId:guid}")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetByCustomerId(Guid customerId)
    {
        var orders = await orderService.GetByCustomerIdAsync(customerId);
        return Ok(orders);
    }

    [HttpGet("courier/{courierId:guid}")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetByCourierId(Guid courierId)
    {
        var orders = await orderService.GetByCourierIdAsync(courierId);
        return Ok(orders);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderDto request)
    {
        var validationResult = await createOrderValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<object>.FailResult("Validation failed", "VALIDATION_ERROR", errors));
        }

        var createdOrder = await orderService.CreateOrderAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, ApiResponse<OrderDto>.SuccessResult(createdOrder));
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Courier,Admin")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderDto dto)
    {
        var validationResult = await updateOrderValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<object>.FailResult("Validation failed", "VALIDATION_ERROR", errors));
        }

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var order = await orderService.UpdateOrderStatusAsync(id, dto, userId);
        return Ok(ApiResponse<OrderDto>.SuccessResult(order));
    }

    [HttpPut("{id:guid}/cancel")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var order = await orderService.CancelOrderAsync(id, userId);
        return Ok(ApiResponse<OrderDto>.SuccessResult(order));
    }

    [HttpGet("{id:guid}/history")]
    public async Task<IActionResult> GetHistory(Guid id)
    {
        var history = await orderService.GetStatusHistoryAsync(id);
        return Ok(ApiResponse<IEnumerable<OrderStatusHistoryDto>>.SuccessResult(history));
    }

    [HttpPost("{id:guid}/rate")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> RateCourier(Guid id, [FromBody] RateCourierDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await orderService.RateCourierAsync(id, dto, userId);
        return Ok(ApiResponse<OrderDto>.SuccessResult(result));
    }
}
