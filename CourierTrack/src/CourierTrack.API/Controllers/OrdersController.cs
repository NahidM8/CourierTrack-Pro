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
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id)
    {
        var order = await orderService.GetByIdAsync(id);
        return order is null ? NotFound() : Ok(order);
    }

    [AllowAnonymous]
    [HttpGet("tracking/{trackingNumber}")]
    public async Task<ActionResult<OrderDto>> GetByTrackingNumber(string trackingNumber)
    {
        var order = await orderService.GetByTrackingNumberAsync(trackingNumber);
        return order is null ? NotFound() : Ok(order);
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
            return BadRequest(validationResult.Errors);

        var createdOrder = await orderService.CreateOrderAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, createdOrder);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(Guid id, [FromBody] UpdateOrderDto request)
    {
        var validationResult = await updateOrderValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        try
        {
            var updatedOrder = await orderService.UpdateOrderStatusAsync(id, request);
            return Ok(updatedOrder);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<ActionResult<OrderDto>> Cancel(Guid id)
    {
        try
        {
            var updatedOrder = await orderService.CancelOrderAsync(id);
            return Ok(updatedOrder);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
