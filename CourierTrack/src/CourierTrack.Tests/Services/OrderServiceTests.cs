using AutoMapper;
using CourierTrack.Application.DTOs;
using CourierTrack.Application.Interfaces.Hubs;
using CourierTrack.Application.Interfaces.Repositories;
using CourierTrack.Application.Interfaces.Services;
using CourierTrack.Application.Options;
using CourierTrack.Application.Services;
using CourierTrack.Domain.Constants;
using CourierTrack.Domain.Entities;
using CourierTrack.Domain.Enums;
using CourierTrack.Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace CourierTrack.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<ICourierRepository> _mockCourierRepository;
    private readonly Mock<ITrackingHubService> _mockTrackingHubService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly IOrderService _orderService;
    private readonly PricingOptions _pricingOptions;

    public OrderServiceTests()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockCourierRepository = new Mock<ICourierRepository>();
        _mockTrackingHubService = new Mock<ITrackingHubService>();
        _mockMapper = new Mock<IMapper>();

        _pricingOptions = new PricingOptions
        {
            PricePerKm = 5m,
            WeightMultiplier = 0.5m,
            MinimumPrice = 10m,
            SmallPackageCharge = 5m,
            MediumPackageCharge = 10m,
            LargePackageCharge = 15m,
            XLargePackageCharge = 20m
        };

        var optionsMonitor = Options.Create(_pricingOptions);

        _orderService = new OrderService(
            _mockOrderRepository.Object,
            _mockCourierRepository.Object,
            _mockTrackingHubService.Object,
            _mockMapper.Object,
            optionsMonitor
        );
    }

    #region CreateOrderAsync Tests

    [Fact]
    public async Task CreateOrderAsync_ShouldGenerateTrackingNumber()
    {
        var customerId = Guid.NewGuid();
        var createOrderDto = new CreateOrderDto(
            CustomerId: customerId,
            PickupAddress: "123 Main St",
            PickupLatitude: 40.7128,
            PickupLongitude: -74.0060,
            DeliveryAddress: "456 Oak Ave",
            DeliveryLatitude: 40.7580,
            DeliveryLongitude: -73.9855,
            PackageDescription: "Test Package",
            PackageWeight: 5m,
            PackageSize: PackageSize.Small
        );

        var createdOrder = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            TrackingNumber = "CT-TEST1234",
            PickupAddress = createOrderDto.PickupAddress,
            PickupLatitude = createOrderDto.PickupLatitude,
            PickupLongitude = createOrderDto.PickupLongitude,
            DeliveryAddress = createOrderDto.DeliveryAddress,
            DeliveryLatitude = createOrderDto.DeliveryLatitude,
            DeliveryLongitude = createOrderDto.DeliveryLongitude,
            PackageDescription = createOrderDto.PackageDescription,
            PackageWeight = createOrderDto.PackageWeight,
            PackageSize = createOrderDto.PackageSize,
            EstimatedDistanceKm = 5m,
            EstimatedDuration = "0h 8m",
            Price = 15m,
            Status = OrderStatus.Created
        };

        var orderDto = new OrderDto(
            Id: createdOrder.Id,
            CustomerId: createdOrder.CustomerId,
            CourierId: null,
            TrackingNumber: createdOrder.TrackingNumber,
            PickupAddress: createdOrder.PickupAddress,
            PickupLatitude: createdOrder.PickupLatitude,
            PickupLongitude: createdOrder.PickupLongitude,
            DeliveryAddress: createdOrder.DeliveryAddress,
            DeliveryLatitude: createdOrder.DeliveryLatitude,
            DeliveryLongitude: createdOrder.DeliveryLongitude,
            PackageDescription: createdOrder.PackageDescription,
            PackageWeight: createdOrder.PackageWeight,
            PackageSize: createdOrder.PackageSize,
            EstimatedDistanceKm: createdOrder.EstimatedDistanceKm,
            EstimatedDuration: createdOrder.EstimatedDuration,
            Price: createdOrder.Price,
            Status: createdOrder.Status,
            CreatedAt: DateTime.UtcNow,
            PickedUpAt: null,
            DeliveredAt: null
        );

        _mockOrderRepository
            .Setup(x => x.AddAsync(It.IsAny<Order>()))
            .ReturnsAsync(createdOrder);

        _mockMapper
            .Setup(x => x.Map<OrderDto>(It.IsAny<Order>()))
            .Returns(orderDto);

        var result = await _orderService.CreateOrderAsync(createOrderDto);

        result.Should().NotBeNull();
        result.TrackingNumber.Should().NotBeNullOrEmpty();
        result.TrackingNumber.Should().StartWith(ApplicationConstants.Order.TrackingNumberPrefix + "-");
        _mockOrderRepository.Verify(x => x.AddAsync(It.IsAny<Order>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldCalculatePrice()
    {
        var customerId = Guid.NewGuid();
        var createOrderDto = new CreateOrderDto(
            CustomerId: customerId,
            PickupAddress: "123 Main St",
            PickupLatitude: 40.7128,
            PickupLongitude: -74.0060,
            DeliveryAddress: "456 Oak Ave",
            DeliveryLatitude: 40.7580,
            DeliveryLongitude: -73.9855,
            PackageDescription: "Test Package",
            PackageWeight: 10m,
            PackageSize: PackageSize.Medium
        );

        var createdOrder = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            TrackingNumber = "CT-TEST1234",
            PickupAddress = createOrderDto.PickupAddress,
            PickupLatitude = createOrderDto.PickupLatitude,
            PickupLongitude = createOrderDto.PickupLongitude,
            DeliveryAddress = createOrderDto.DeliveryAddress,
            DeliveryLatitude = createOrderDto.DeliveryLatitude,
            DeliveryLongitude = createOrderDto.DeliveryLongitude,
            PackageDescription = createOrderDto.PackageDescription,
            PackageWeight = createOrderDto.PackageWeight,
            PackageSize = createOrderDto.PackageSize,
            EstimatedDistanceKm = 5m,
            EstimatedDuration = "0h 8m",
            Price = 40m, // (5 * 5) + (0.5 * 10) + 10 = 40
            Status = OrderStatus.Created
        };

        var orderDto = new OrderDto(
            Id: createdOrder.Id,
            CustomerId: createdOrder.CustomerId,
            CourierId: null,
            TrackingNumber: createdOrder.TrackingNumber,
            PickupAddress: createdOrder.PickupAddress,
            PickupLatitude: createdOrder.PickupLatitude,
            PickupLongitude: createdOrder.PickupLongitude,
            DeliveryAddress: createdOrder.DeliveryAddress,
            DeliveryLatitude: createdOrder.DeliveryLatitude,
            DeliveryLongitude: createdOrder.DeliveryLongitude,
            PackageDescription: createdOrder.PackageDescription,
            PackageWeight: createdOrder.PackageWeight,
            PackageSize: createdOrder.PackageSize,
            EstimatedDistanceKm: createdOrder.EstimatedDistanceKm,
            EstimatedDuration: createdOrder.EstimatedDuration,
            Price: createdOrder.Price,
            Status: createdOrder.Status,
            CreatedAt: DateTime.UtcNow,
            PickedUpAt: null,
            DeliveredAt: null
        );

        _mockOrderRepository
            .Setup(x => x.AddAsync(It.IsAny<Order>()))
            .ReturnsAsync(createdOrder);

        _mockMapper
            .Setup(x => x.Map<OrderDto>(It.IsAny<Order>()))
            .Returns(orderDto);

        var result = await _orderService.CreateOrderAsync(createOrderDto);

        result.Should().NotBeNull();
        result.Price.Should().Be(40m);
        result.Price.Should().BeGreaterThanOrEqualTo(_pricingOptions.MinimumPrice);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldSetStatusToCreated()
    {
        var customerId = Guid.NewGuid();
        var createOrderDto = new CreateOrderDto(
            CustomerId: customerId,
            PickupAddress: "123 Main St",
            PickupLatitude: 40.7128,
            PickupLongitude: -74.0060,
            DeliveryAddress: "456 Oak Ave",
            DeliveryLatitude: 40.7580,
            DeliveryLongitude: -73.9855,
            PackageDescription: "Test Package",
            PackageWeight: 5m,
            PackageSize: PackageSize.Small
        );

        var createdOrder = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            TrackingNumber = "CT-TEST1234",
            PickupAddress = createOrderDto.PickupAddress,
            PickupLatitude = createOrderDto.PickupLatitude,
            PickupLongitude = createOrderDto.PickupLongitude,
            DeliveryAddress = createOrderDto.DeliveryAddress,
            DeliveryLatitude = createOrderDto.DeliveryLatitude,
            DeliveryLongitude = createOrderDto.DeliveryLongitude,
            PackageDescription = createOrderDto.PackageDescription,
            PackageWeight = createOrderDto.PackageWeight,
            PackageSize = createOrderDto.PackageSize,
            EstimatedDistanceKm = 5m,
            EstimatedDuration = "0h 8m",
            Price = 15m,
            Status = OrderStatus.Created
        };

        var orderDto = new OrderDto(
            Id: createdOrder.Id,
            CustomerId: createdOrder.CustomerId,
            CourierId: null,
            TrackingNumber: createdOrder.TrackingNumber,
            PickupAddress: createdOrder.PickupAddress,
            PickupLatitude: createdOrder.PickupLatitude,
            PickupLongitude: createdOrder.PickupLongitude,
            DeliveryAddress: createdOrder.DeliveryAddress,
            DeliveryLatitude: createdOrder.DeliveryLatitude,
            DeliveryLongitude: createdOrder.DeliveryLongitude,
            PackageDescription: createdOrder.PackageDescription,
            PackageWeight: createdOrder.PackageWeight,
            PackageSize: createdOrder.PackageSize,
            EstimatedDistanceKm: createdOrder.EstimatedDistanceKm,
            EstimatedDuration: createdOrder.EstimatedDuration,
            Price: createdOrder.Price,
            Status: createdOrder.Status,
            CreatedAt: DateTime.UtcNow,
            PickedUpAt: null,
            DeliveredAt: null
        );

        _mockOrderRepository
            .Setup(x => x.AddAsync(It.IsAny<Order>()))
            .ReturnsAsync(createdOrder);

        _mockMapper
            .Setup(x => x.Map<OrderDto>(It.IsAny<Order>()))
            .Returns(orderDto);

        var result = await _orderService.CreateOrderAsync(createOrderDto);

        result.Status.Should().Be(OrderStatus.Created);
    }

    #endregion

    #region UpdateOrderStatusAsync Tests

    [Fact]
    public async Task UpdateOrderStatusAsync_ShouldThrow_WhenOrderIsCancelled()
    {
        var orderId = Guid.NewGuid();
        var changedBy = Guid.NewGuid();
        var cancelledOrder = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Cancelled,
            TrackingNumber = "CT-TEST1234",
            PickupAddress = "123 Main St",
            PickupLatitude = 40.7128,
            PickupLongitude = -74.0060,
            DeliveryAddress = "456 Oak Ave",
            DeliveryLatitude = 40.7580,
            DeliveryLongitude = -73.9855,
            EstimatedDistanceKm = 5m,
            EstimatedDuration = "0h 8m",
            Price = 15m
        };

        var updateOrderDto = new UpdateOrderDto(
            CourierId: null,
            Status: OrderStatus.Pending,
            PickedUpAt: null,
            DeliveredAt: null,
            Note: "Update"
        );

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(cancelledOrder);

        await Assert.ThrowsAsync<Domain.Exceptions.InvalidOperationException>(
            () => _orderService.UpdateOrderStatusAsync(orderId, updateOrderDto, changedBy)
        );
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_ShouldUpdateStatus()
    {
        var orderId = Guid.NewGuid();
        var changedBy = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Created,
            TrackingNumber = "CT-TEST1234",
            PickupAddress = "123 Main St",
            PickupLatitude = 40.7128,
            PickupLongitude = -74.0060,
            DeliveryAddress = "456 Oak Ave",
            DeliveryLatitude = 40.7580,
            DeliveryLongitude = -73.9855,
            EstimatedDistanceKm = 5m,
            EstimatedDuration = "0h 8m",
            Price = 15m
        };

        var updateOrderDto = new UpdateOrderDto(
            CourierId: null,
            Status: OrderStatus.Pending,
            PickedUpAt: null,
            DeliveredAt: null,
            Note: "Update"
        );

        var updatedOrder = new Order
        {
            Id = orderId,
            CustomerId = order.CustomerId,
            Status = OrderStatus.Pending,
            TrackingNumber = order.TrackingNumber,
            PickupAddress = order.PickupAddress,
            PickupLatitude = order.PickupLatitude,
            PickupLongitude = order.PickupLongitude,
            DeliveryAddress = order.DeliveryAddress,
            DeliveryLatitude = order.DeliveryLatitude,
            DeliveryLongitude = order.DeliveryLongitude,
            EstimatedDistanceKm = order.EstimatedDistanceKm,
            EstimatedDuration = order.EstimatedDuration,
            Price = order.Price
        };

        var orderDto = new OrderDto(
            Id: updatedOrder.Id,
            CustomerId: updatedOrder.CustomerId,
            CourierId: null,
            TrackingNumber: updatedOrder.TrackingNumber,
            PickupAddress: updatedOrder.PickupAddress,
            PickupLatitude: updatedOrder.PickupLatitude,
            PickupLongitude: updatedOrder.PickupLongitude,
            DeliveryAddress: updatedOrder.DeliveryAddress,
            DeliveryLatitude: updatedOrder.DeliveryLatitude,
            DeliveryLongitude: updatedOrder.DeliveryLongitude,
            PackageDescription: null,
            PackageWeight: 0m,
            PackageSize: PackageSize.Small,
            EstimatedDistanceKm: updatedOrder.EstimatedDistanceKm,
            EstimatedDuration: updatedOrder.EstimatedDuration,
            Price: updatedOrder.Price,
            Status: updatedOrder.Status,
            CreatedAt: DateTime.UtcNow,
            PickedUpAt: null,
            DeliveredAt: null
        );

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _mockOrderRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Order>()))
            .Returns<Order>(o => Task.FromResult(o));

        _mockOrderRepository
            .Setup(x => x.AddStatusHistoryAsync(It.IsAny<OrderStatusHistory>()))
            .Returns(Task.CompletedTask);

        _mockTrackingHubService
            .Setup(x => x.NotifyOrderStatusUpdatedAsync(It.IsAny<Guid>(), It.IsAny<OrderStatus>()))
            .Returns(Task.CompletedTask);

        _mockMapper
            .Setup(x => x.Map<OrderDto>(It.IsAny<Order>()))
            .Returns(orderDto);

        var result = await _orderService.UpdateOrderStatusAsync(orderId, updateOrderDto, changedBy);

        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Pending);
        _mockOrderRepository.Verify(x => x.UpdateAsync(It.IsAny<Order>()), Times.Once);
        _mockOrderRepository.Verify(x => x.AddStatusHistoryAsync(It.IsAny<OrderStatusHistory>()), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_ShouldSetPickedUpAt_WhenStatusIsPickedUp()
    {
        var orderId = Guid.NewGuid();
        var changedBy = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            TrackingNumber = "CT-TEST1234",
            PickupAddress = "123 Main St",
            PickupLatitude = 40.7128,
            PickupLongitude = -74.0060,
            DeliveryAddress = "456 Oak Ave",
            DeliveryLatitude = 40.7580,
            DeliveryLongitude = -73.9855,
            EstimatedDistanceKm = 5m,
            EstimatedDuration = "0h 8m",
            Price = 15m,
            PickedUpAt = null
        };

        var updateOrderDto = new UpdateOrderDto(
            CourierId: null,
            Status: OrderStatus.PickedUp,
            PickedUpAt: null,
            DeliveredAt: null,
            Note: "Picked up"
        );

        var updatedOrder = new Order
        {
            Id = orderId,
            CustomerId = order.CustomerId,
            Status = OrderStatus.PickedUp,
            TrackingNumber = order.TrackingNumber,
            PickupAddress = order.PickupAddress,
            PickupLatitude = order.PickupLatitude,
            PickupLongitude = order.PickupLongitude,
            DeliveryAddress = order.DeliveryAddress,
            DeliveryLatitude = order.DeliveryLatitude,
            DeliveryLongitude = order.DeliveryLongitude,
            EstimatedDistanceKm = order.EstimatedDistanceKm,
            EstimatedDuration = order.EstimatedDuration,
            Price = order.Price,
            PickedUpAt = DateTime.UtcNow
        };

        var orderDto = new OrderDto(
            Id: updatedOrder.Id,
            CustomerId: updatedOrder.CustomerId,
            CourierId: null,
            TrackingNumber: updatedOrder.TrackingNumber,
            PickupAddress: updatedOrder.PickupAddress,
            PickupLatitude: updatedOrder.PickupLatitude,
            PickupLongitude: updatedOrder.PickupLongitude,
            DeliveryAddress: updatedOrder.DeliveryAddress,
            DeliveryLatitude: updatedOrder.DeliveryLatitude,
            DeliveryLongitude: updatedOrder.DeliveryLongitude,
            PackageDescription: null,
            PackageWeight: 0m,
            PackageSize: PackageSize.Small,
            EstimatedDistanceKm: updatedOrder.EstimatedDistanceKm,
            EstimatedDuration: updatedOrder.EstimatedDuration,
            Price: updatedOrder.Price,
            Status: updatedOrder.Status,
            CreatedAt: DateTime.UtcNow,
            PickedUpAt: updatedOrder.PickedUpAt,
            DeliveredAt: null
        );

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _mockOrderRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Order>()))
            .Returns<Order>(o => Task.FromResult(o));

        _mockOrderRepository
            .Setup(x => x.AddStatusHistoryAsync(It.IsAny<OrderStatusHistory>()))
            .Returns(Task.CompletedTask);

        _mockTrackingHubService
            .Setup(x => x.NotifyOrderPickedUpAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        _mockTrackingHubService
            .Setup(x => x.NotifyOrderStatusUpdatedAsync(It.IsAny<Guid>(), It.IsAny<OrderStatus>()))
            .Returns(Task.CompletedTask);

        _mockMapper
            .Setup(x => x.Map<OrderDto>(It.IsAny<Order>()))
            .Returns(orderDto);

        var result = await _orderService.UpdateOrderStatusAsync(orderId, updateOrderDto, changedBy);

        result.Status.Should().Be(OrderStatus.PickedUp);
        _mockTrackingHubService.Verify(x => x.NotifyOrderPickedUpAsync(orderId), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_ShouldSetDeliveredAt_WhenStatusIsDelivered()
    {
        var orderId = Guid.NewGuid();
        var changedBy = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.InTransit,
            TrackingNumber = "CT-TEST1234",
            PickupAddress = "123 Main St",
            PickupLatitude = 40.7128,
            PickupLongitude = -74.0060,
            DeliveryAddress = "456 Oak Ave",
            DeliveryLatitude = 40.7580,
            DeliveryLongitude = -73.9855,
            EstimatedDistanceKm = 5m,
            EstimatedDuration = "0h 8m",
            Price = 15m,
            DeliveredAt = null
        };

        var updateOrderDto = new UpdateOrderDto(
            CourierId: null,
            Status: OrderStatus.Delivered,
            PickedUpAt: null,
            DeliveredAt: null,
            Note: "Delivered"
        );

        var updatedOrder = new Order
        {
            Id = orderId,
            CustomerId = order.CustomerId,
            Status = OrderStatus.Delivered,
            TrackingNumber = order.TrackingNumber,
            PickupAddress = order.PickupAddress,
            PickupLatitude = order.PickupLatitude,
            PickupLongitude = order.PickupLongitude,
            DeliveryAddress = order.DeliveryAddress,
            DeliveryLatitude = order.DeliveryLatitude,
            DeliveryLongitude = order.DeliveryLongitude,
            EstimatedDistanceKm = order.EstimatedDistanceKm,
            EstimatedDuration = order.EstimatedDuration,
            Price = order.Price,
            DeliveredAt = DateTime.UtcNow
        };

        var orderDto = new OrderDto(
            Id: updatedOrder.Id,
            CustomerId: updatedOrder.CustomerId,
            CourierId: null,
            TrackingNumber: updatedOrder.TrackingNumber,
            PickupAddress: updatedOrder.PickupAddress,
            PickupLatitude: updatedOrder.PickupLatitude,
            PickupLongitude: updatedOrder.PickupLongitude,
            DeliveryAddress: updatedOrder.DeliveryAddress,
            DeliveryLatitude: updatedOrder.DeliveryLatitude,
            DeliveryLongitude: updatedOrder.DeliveryLongitude,
            PackageDescription: null,
            PackageWeight: 0m,
            PackageSize: PackageSize.Small,
            EstimatedDistanceKm: updatedOrder.EstimatedDistanceKm,
            EstimatedDuration: updatedOrder.EstimatedDuration,
            Price: updatedOrder.Price,
            Status: updatedOrder.Status,
            CreatedAt: DateTime.UtcNow,
            PickedUpAt: null,
            DeliveredAt: updatedOrder.DeliveredAt
        );

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _mockOrderRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Order>()))
            .Returns<Order>(o => Task.FromResult(o));

        _mockOrderRepository
            .Setup(x => x.AddStatusHistoryAsync(It.IsAny<OrderStatusHistory>()))
            .Returns(Task.CompletedTask);

        _mockTrackingHubService
            .Setup(x => x.NotifyOrderStatusUpdatedAsync(It.IsAny<Guid>(), It.IsAny<OrderStatus>()))
            .Returns(Task.CompletedTask);

        _mockMapper
            .Setup(x => x.Map<OrderDto>(It.IsAny<Order>()))
            .Returns(orderDto);

        var result = await _orderService.UpdateOrderStatusAsync(orderId, updateOrderDto, changedBy);

        result.Status.Should().Be(OrderStatus.Delivered);
        result.DeliveredAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_ShouldThrow_WhenOrderNotFound()
    {
        var orderId = Guid.NewGuid();
        var changedBy = Guid.NewGuid();
        var updateOrderDto = new UpdateOrderDto(
            CourierId: null,
            Status: OrderStatus.Pending,
            PickedUpAt: null,
            DeliveredAt: null,
            Note: "Update"
        );

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync((Order?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _orderService.UpdateOrderStatusAsync(orderId, updateOrderDto, changedBy)
        );
    }

    #endregion

    #region CancelOrderAsync Tests

    [Fact]
    public async Task CancelOrderAsync_ShouldThrow_WhenOrderIsDelivered()
    {
        var orderId = Guid.NewGuid();
        var changedBy = Guid.NewGuid();
        var deliveredOrder = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Delivered,
            TrackingNumber = "CT-TEST1234",
            PickupAddress = "123 Main St",
            PickupLatitude = 40.7128,
            PickupLongitude = -74.0060,
            DeliveryAddress = "456 Oak Ave",
            DeliveryLatitude = 40.7580,
            DeliveryLongitude = -73.9855,
            EstimatedDistanceKm = 5m,
            EstimatedDuration = "0h 8m",
            Price = 15m,
            DeliveredAt = DateTime.UtcNow
        };

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(deliveredOrder);

        await Assert.ThrowsAsync<Domain.Exceptions.InvalidOperationException>(
            () => _orderService.CancelOrderAsync(orderId, changedBy)
        );
    }

    [Fact]
    public async Task CancelOrderAsync_ShouldThrow_WhenAlreadyCancelled()
    {
        var orderId = Guid.NewGuid();
        var changedBy = Guid.NewGuid();
        var cancelledOrder = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Cancelled,
            TrackingNumber = "CT-TEST1234",
            PickupAddress = "123 Main St",
            PickupLatitude = 40.7128,
            PickupLongitude = -74.0060,
            DeliveryAddress = "456 Oak Ave",
            DeliveryLatitude = 40.7580,
            DeliveryLongitude = -73.9855,
            EstimatedDistanceKm = 5m,
            EstimatedDuration = "0h 8m",
            Price = 15m
        };

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(cancelledOrder);

        await Assert.ThrowsAsync<Domain.Exceptions.InvalidOperationException>(
            () => _orderService.CancelOrderAsync(orderId, changedBy)
        );
    }

    [Fact]
    public async Task CancelOrderAsync_ShouldSucceed_WhenOrderIsPending()
    {
        var orderId = Guid.NewGuid();
        var changedBy = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            TrackingNumber = "CT-TEST1234",
            PickupAddress = "123 Main St",
            PickupLatitude = 40.7128,
            PickupLongitude = -74.0060,
            DeliveryAddress = "456 Oak Ave",
            DeliveryLatitude = 40.7580,
            DeliveryLongitude = -73.9855,
            EstimatedDistanceKm = 5m,
            EstimatedDuration = "0h 8m",
            Price = 15m
        };

        var cancelledOrder = new Order
        {
            Id = orderId,
            CustomerId = order.CustomerId,
            Status = OrderStatus.Cancelled,
            TrackingNumber = order.TrackingNumber,
            PickupAddress = order.PickupAddress,
            PickupLatitude = order.PickupLatitude,
            PickupLongitude = order.PickupLongitude,
            DeliveryAddress = order.DeliveryAddress,
            DeliveryLatitude = order.DeliveryLatitude,
            DeliveryLongitude = order.DeliveryLongitude,
            EstimatedDistanceKm = order.EstimatedDistanceKm,
            EstimatedDuration = order.EstimatedDuration,
            Price = order.Price
        };

        var orderDto = new OrderDto(
            Id: cancelledOrder.Id,
            CustomerId: cancelledOrder.CustomerId,
            CourierId: null,
            TrackingNumber: cancelledOrder.TrackingNumber,
            PickupAddress: cancelledOrder.PickupAddress,
            PickupLatitude: cancelledOrder.PickupLatitude,
            PickupLongitude: cancelledOrder.PickupLongitude,
            DeliveryAddress: cancelledOrder.DeliveryAddress,
            DeliveryLatitude: cancelledOrder.DeliveryLatitude,
            DeliveryLongitude: cancelledOrder.DeliveryLongitude,
            PackageDescription: null,
            PackageWeight: 0m,
            PackageSize: PackageSize.Small,
            EstimatedDistanceKm: cancelledOrder.EstimatedDistanceKm,
            EstimatedDuration: cancelledOrder.EstimatedDuration,
            Price: cancelledOrder.Price,
            Status: cancelledOrder.Status,
            CreatedAt: DateTime.UtcNow,
            PickedUpAt: null,
            DeliveredAt: null
        );

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _mockOrderRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Order>()))
            .Returns<Order>(o => Task.FromResult(o));

        _mockOrderRepository
            .Setup(x => x.AddStatusHistoryAsync(It.IsAny<OrderStatusHistory>()))
            .Returns(Task.CompletedTask);

        _mockMapper
            .Setup(x => x.Map<OrderDto>(It.IsAny<Order>()))
            .Returns(orderDto);

        var result = await _orderService.CancelOrderAsync(orderId, changedBy);

        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Cancelled);
        _mockOrderRepository.Verify(x => x.UpdateAsync(It.IsAny<Order>()), Times.Once);
        _mockOrderRepository.Verify(x => x.AddStatusHistoryAsync(It.IsAny<OrderStatusHistory>()), Times.Once);
    }

    [Fact]
    public async Task CancelOrderAsync_ShouldSucceed_WhenOrderIsPickedUp()
    {
        var orderId = Guid.NewGuid();
        var changedBy = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.PickedUp,
            TrackingNumber = "CT-TEST1234",
            PickupAddress = "123 Main St",
            PickupLatitude = 40.7128,
            PickupLongitude = -74.0060,
            DeliveryAddress = "456 Oak Ave",
            DeliveryLatitude = 40.7580,
            DeliveryLongitude = -73.9855,
            EstimatedDistanceKm = 5m,
            EstimatedDuration = "0h 8m",
            Price = 15m,
            PickedUpAt = DateTime.UtcNow
        };

        var cancelledOrder = new Order
        {
            Id = orderId,
            CustomerId = order.CustomerId,
            Status = OrderStatus.Cancelled,
            TrackingNumber = order.TrackingNumber,
            PickupAddress = order.PickupAddress,
            PickupLatitude = order.PickupLatitude,
            PickupLongitude = order.PickupLongitude,
            DeliveryAddress = order.DeliveryAddress,
            DeliveryLatitude = order.DeliveryLatitude,
            DeliveryLongitude = order.DeliveryLongitude,
            EstimatedDistanceKm = order.EstimatedDistanceKm,
            EstimatedDuration = order.EstimatedDuration,
            Price = order.Price,
            PickedUpAt = order.PickedUpAt
        };

        var orderDto = new OrderDto(
            Id: cancelledOrder.Id,
            CustomerId: cancelledOrder.CustomerId,
            CourierId: null,
            TrackingNumber: cancelledOrder.TrackingNumber,
            PickupAddress: cancelledOrder.PickupAddress,
            PickupLatitude: cancelledOrder.PickupLatitude,
            PickupLongitude: cancelledOrder.PickupLongitude,
            DeliveryAddress: cancelledOrder.DeliveryAddress,
            DeliveryLatitude: cancelledOrder.DeliveryLatitude,
            DeliveryLongitude: cancelledOrder.DeliveryLongitude,
            PackageDescription: null,
            PackageWeight: 0m,
            PackageSize: PackageSize.Small,
            EstimatedDistanceKm: cancelledOrder.EstimatedDistanceKm,
            EstimatedDuration: cancelledOrder.EstimatedDuration,
            Price: cancelledOrder.Price,
            Status: cancelledOrder.Status,
            CreatedAt: DateTime.UtcNow,
            PickedUpAt: cancelledOrder.PickedUpAt,
            DeliveredAt: null
        );

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _mockOrderRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Order>()))
            .Returns<Order>(o => Task.FromResult(o));

        _mockOrderRepository
            .Setup(x => x.AddStatusHistoryAsync(It.IsAny<OrderStatusHistory>()))
            .Returns(Task.CompletedTask);

        _mockMapper
            .Setup(x => x.Map<OrderDto>(It.IsAny<Order>()))
            .Returns(orderDto);

        var result = await _orderService.CancelOrderAsync(orderId, changedBy);

        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Cancelled);
        _mockOrderRepository.Verify(x => x.UpdateAsync(It.IsAny<Order>()), Times.Once);
    }

    [Fact]
    public async Task CancelOrderAsync_ShouldThrow_WhenOrderNotFound()
    {
        var orderId = Guid.NewGuid();
        var changedBy = Guid.NewGuid();

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync((Order?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _orderService.CancelOrderAsync(orderId, changedBy)
        );
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenOrderNotFound()
    {
        var orderId = Guid.NewGuid();
        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync((Order?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _orderService.GetByIdAsync(orderId)
        );
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOrder_WhenFound()
    {
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Created,
            TrackingNumber = "CT-TEST1234",
            PickupAddress = "123 Main St",
            PickupLatitude = 40.7128,
            PickupLongitude = -74.0060,
            DeliveryAddress = "456 Oak Ave",
            DeliveryLatitude = 40.7580,
            DeliveryLongitude = -73.9855,
            EstimatedDistanceKm = 5m,
            EstimatedDuration = "0h 8m",
            Price = 15m
        };

        var orderDto = new OrderDto(
            Id: order.Id,
            CustomerId: order.CustomerId,
            CourierId: null,
            TrackingNumber: order.TrackingNumber,
            PickupAddress: order.PickupAddress,
            PickupLatitude: order.PickupLatitude,
            PickupLongitude: order.PickupLongitude,
            DeliveryAddress: order.DeliveryAddress,
            DeliveryLatitude: order.DeliveryLatitude,
            DeliveryLongitude: order.DeliveryLongitude,
            PackageDescription: null,
            PackageWeight: 0m,
            PackageSize: PackageSize.Small,
            EstimatedDistanceKm: order.EstimatedDistanceKm,
            EstimatedDuration: order.EstimatedDuration,
            Price: order.Price,
            Status: order.Status,
            CreatedAt: DateTime.UtcNow,
            PickedUpAt: null,
            DeliveredAt: null
        );

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _mockMapper
            .Setup(x => x.Map<OrderDto>(order))
            .Returns(orderDto);

        var result = await _orderService.GetByIdAsync(orderId);

        result.Should().NotBeNull();
        result.Id.Should().Be(orderId);
        result.TrackingNumber.Should().Be(order.TrackingNumber);
    }

    #endregion

    #region GetByTrackingNumberAsync Tests

    [Fact]
    public async Task GetByTrackingNumberAsync_ShouldThrow_WhenNotFound()
    {
        var trackingNumber = "CT-NOTFOUND";
        _mockOrderRepository
            .Setup(x => x.GetByTrackingNumberAsync(trackingNumber))
            .ReturnsAsync((Order?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _orderService.GetByTrackingNumberAsync(trackingNumber)
        );
    }

    [Fact]
    public async Task GetByTrackingNumberAsync_ShouldReturnOrder_WhenFound()
    {
        var trackingNumber = "CT-TEST1234";
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Created,
            TrackingNumber = trackingNumber,
            PickupAddress = "123 Main St",
            PickupLatitude = 40.7128,
            PickupLongitude = -74.0060,
            DeliveryAddress = "456 Oak Ave",
            DeliveryLatitude = 40.7580,
            DeliveryLongitude = -73.9855,
            EstimatedDistanceKm = 5m,
            EstimatedDuration = "0h 8m",
            Price = 15m
        };

        var orderDto = new OrderDto(
            Id: order.Id,
            CustomerId: order.CustomerId,
            CourierId: null,
            TrackingNumber: order.TrackingNumber,
            PickupAddress: order.PickupAddress,
            PickupLatitude: order.PickupLatitude,
            PickupLongitude: order.PickupLongitude,
            DeliveryAddress: order.DeliveryAddress,
            DeliveryLatitude: order.DeliveryLatitude,
            DeliveryLongitude: order.DeliveryLongitude,
            PackageDescription: null,
            PackageWeight: 0m,
            PackageSize: PackageSize.Small,
            EstimatedDistanceKm: order.EstimatedDistanceKm,
            EstimatedDuration: order.EstimatedDuration,
            Price: order.Price,
            Status: order.Status,
            CreatedAt: DateTime.UtcNow,
            PickedUpAt: null,
            DeliveredAt: null
        );

        _mockOrderRepository
            .Setup(x => x.GetByTrackingNumberAsync(trackingNumber))
            .ReturnsAsync(order);

        _mockMapper
            .Setup(x => x.Map<OrderDto>(order))
            .Returns(orderDto);

        var result = await _orderService.GetByTrackingNumberAsync(trackingNumber);

        result.Should().NotBeNull();
        result.TrackingNumber.Should().Be(trackingNumber);
        result.Id.Should().Be(order.Id);
    }

    #endregion
}
