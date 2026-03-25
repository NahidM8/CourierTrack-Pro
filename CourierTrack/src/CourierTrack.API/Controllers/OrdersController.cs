using AutoMapper;
using CourierTrack.Application.DTOs;
using CourierTrack.Domain.Entities;
using CourierTrack.Domain.Enums;
using CourierTrack.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CourierTrack.API.Controllers;

[Route("api/v1/orders")]
[ApiController]
public class OrdersController(IOrderRepository orderRepository, IMapper mapper, IConfiguration configuration) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll()
    {
        var orders = await orderRepository.GetAllAsync();
        return Ok(mapper.Map<IEnumerable<OrderDto>>(orders));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id)
    {
        var order = await orderRepository.GetByIdAsync(id);
        return order is null ? NotFound() : Ok(mapper.Map<OrderDto>(order));
    }

    [HttpGet("tracking/{trackingNumber}")]
    public async Task<ActionResult<OrderDto>> GetByTrackingNumber(string trackingNumber)
    {
        var order = await orderRepository.GetByTrackingNumberAsync(trackingNumber);
        return order is null ? NotFound() : Ok(mapper.Map<OrderDto>(order));
    }

    [HttpGet("customer/{customerId:guid}")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetByCustomerId(Guid customerId)
    {
        var orders = await orderRepository.GetByCustomerIdAsync(customerId);
        return Ok(mapper.Map<IEnumerable<OrderDto>>(orders));
    }

    [HttpGet("courier/{courierId:guid}")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetByCourierId(Guid courierId)
    {
        var orders = await orderRepository.GetByCourierIdAsync(courierId);
        return Ok(mapper.Map<IEnumerable<OrderDto>>(orders));
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderDto request)
    {
        var order = mapper.Map<Order>(request);

        var estimatedDistanceKm = CalculateDistanceKm(
            request.PickupLatitude,
            request.PickupLongitude,
            request.DeliveryLatitude,
            request.DeliveryLongitude);

        order.Id = Guid.NewGuid();
        order.TrackingNumber = GenerateTrackingNumber();
        order.CourierId = null;
        order.EstimatedDistanceKm = estimatedDistanceKm;
        order.EstimatedDuration = CalculateEstimatedDuration(estimatedDistanceKm);
        order.Price = CalculatePrice(estimatedDistanceKm, request.PackageWeight, request.PackageSize);
        order.Status = OrderStatus.Created;

        var createdOrder = await orderRepository.AddAsync(order);
        var response = mapper.Map<OrderDto>(createdOrder);

        return CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, response);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(Guid id, [FromBody] UpdateOrderDto request)
    {
        var order = await orderRepository.GetByIdAsync(id);
        if (order is null)
            return NotFound();

        if (order.Status == OrderStatus.Cancelled)
            return BadRequest("Cancelled orders cannot be updated.");

        mapper.Map(request, order);

        if (order.Status == OrderStatus.PickedUp && order.PickedUpAt is null)
            order.PickedUpAt = DateTime.UtcNow;

        if (order.Status == OrderStatus.Delivered && order.DeliveredAt is null)
            order.DeliveredAt = DateTime.UtcNow;

        var updatedOrder = await orderRepository.UpdateAsync(order);
        return Ok(mapper.Map<OrderDto>(updatedOrder));
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<ActionResult<OrderDto>> Cancel(Guid id)
    {
        var order = await orderRepository.GetByIdAsync(id);
        if (order is null)
            return NotFound();

        if (order.Status == OrderStatus.Delivered)
            return BadRequest("Delivered orders cannot be cancelled.");

        if (order.Status == OrderStatus.Cancelled)
            return BadRequest("Order is already cancelled.");

        order.Status = OrderStatus.Cancelled;
        order.DeliveredAt = null;

        var updatedOrder = await orderRepository.UpdateAsync(order);
        return Ok(mapper.Map<OrderDto>(updatedOrder));
    }

    private static string GenerateTrackingNumber()
    {
        var randomNumber = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        return $"CT-{randomNumber}";
    }

    private static decimal CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusKm = 6371;
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2))
                * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return Math.Round((decimal)(earthRadiusKm * c), 2);
    }

    private static double DegreesToRadians(double degrees) => degrees * (Math.PI / 180.0);

    private static string CalculateEstimatedDuration(decimal distanceKm)
    {
        const decimal averageSpeedKmPerHour = 35m;
        var totalMinutes = Math.Ceiling((distanceKm / averageSpeedKmPerHour) * 60);
        var hours = (int)totalMinutes / 60;
        var minutes = (int)totalMinutes % 60;
        return $"{hours}h {minutes}m";
    }

    private decimal CalculatePrice(decimal distanceKm, decimal packageWeight, PackageSize packageSize)
    {
        // Price formula from image: Fiyat = (Mesafe x KmBasınaFiyat) + (AğırlıklıKatsayısı x Ağırlık) + BoyutEkFiyatı
        var pricePenka = configuration.GetValue<decimal>("PricingService:PricePenka");
        var weightMultiplier = configuration.GetValue<decimal>("PricingService:WeightMultiplier");
        var minimumPrice = configuration.GetValue<decimal>("PricingService:MinimumPrice");

        var sizeCharge = packageSize switch
        {
            PackageSize.Small => configuration.GetValue<decimal>("PricingService:PackageSizes:Small"),
            PackageSize.Medium => configuration.GetValue<decimal>("PricingService:PackageSizes:Medium"),
            PackageSize.Large => configuration.GetValue<decimal>("PricingService:PackageSizes:Large"),
            PackageSize.XLarge => configuration.GetValue<decimal>("PricingService:PackageSizes:XLarge"),
            _ => 0m
        };

        var totalPrice = (distanceKm * pricePenka) + (weightMultiplier * packageWeight) + sizeCharge;
        return Math.Round(Math.Max(totalPrice, minimumPrice), 2);
    }
}
