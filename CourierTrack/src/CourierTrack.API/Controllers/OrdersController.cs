using CourierTrack.Domain.Entities;
using CourierTrack.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CourierTrack.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController(IOrderRepository orderRepository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetAll()
    {
        var orders = await orderRepository.GetAllAsync();
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Order>> GetById(Guid id)
    {
        var order = await orderRepository.GetByIdAsync(id);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpGet("tracking/{trackingNumber}")]
    public async Task<ActionResult<Order>> GetByTrackingNumber(string trackingNumber)
    {
        var order = await orderRepository.GetByTrackingNumberAsync(trackingNumber);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpGet("customer/{customerId:guid}")]
    public async Task<ActionResult<IEnumerable<Order>>> GetByCustomerId(Guid customerId)
    {
        var orders = await orderRepository.GetByCustomerIdAsync(customerId);
        return Ok(orders);
    }

    [HttpGet("courier/{courierId:guid}")]
    public async Task<ActionResult<IEnumerable<Order>>> GetByCourierId(Guid courierId)
    {
        var orders = await orderRepository.GetByCourierIdAsync(courierId);
        return Ok(orders);
    }
}
