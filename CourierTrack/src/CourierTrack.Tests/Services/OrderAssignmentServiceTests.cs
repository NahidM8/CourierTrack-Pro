using AutoMapper;
using CourierTrack.Application.DTOs;
using CourierTrack.Application.Interfaces.Hubs;
using CourierTrack.Application.Interfaces.Repositories;
using CourierTrack.Application.Interfaces.Services;
using CourierTrack.Application.Services;
using CourierTrack.Domain.Entities;
using CourierTrack.Domain.Enums;
using CourierTrack.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace CourierTrack.Tests.Services;

public class OrderAssignmentServiceTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<ICourierRepository> _mockCourierRepository;
    private readonly Mock<ITrackingHubService> _mockTrackingHubService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly IOrderAssignmentService _orderAssignmentService;

    public OrderAssignmentServiceTests()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockCourierRepository = new Mock<ICourierRepository>();
        _mockTrackingHubService = new Mock<ITrackingHubService>();
        _mockMapper = new Mock<IMapper>();

        _orderAssignmentService = new OrderAssignmentService(
            _mockOrderRepository.Object,
            _mockCourierRepository.Object,
            _mockTrackingHubService.Object,
            _mockMapper.Object
        );
    }

    #region AssignOrderAsync Tests

    [Fact]
    public async Task AssignOrderAsync_ShouldRecordStatusHistory_OnAssignment()
    {
        var orderId = Guid.NewGuid();
        var courierId = Guid.NewGuid();
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
            PackageWeight = 5m,
            PackageSize = PackageSize.Small,
            EstimatedDistanceKm = 5m,
            EstimatedDuration = "0h 8m",
            Price = 15m
        };

        var courier = new Courier
        {
            Id = courierId,
            UserId = Guid.NewGuid(),
            VehicleType = VehicleType.Motorcycle,
            CurrentLatitude = 40.7400,
            CurrentLongitude = -73.9700,
            IsAvailable = true,
            Rating = 4.5m,
            TotalDeliveries = 100
        };

        var assignedOrder = new Order
        {
            Id = orderId,
            CustomerId = order.CustomerId,
            CourierId = courierId,
            Status = OrderStatus.Assigned,
            TrackingNumber = order.TrackingNumber,
            PickupAddress = order.PickupAddress,
            PickupLatitude = order.PickupLatitude,
            PickupLongitude = order.PickupLongitude,
            DeliveryAddress = order.DeliveryAddress,
            DeliveryLatitude = order.DeliveryLatitude,
            DeliveryLongitude = order.DeliveryLongitude,
            PackageWeight = order.PackageWeight,
            PackageSize = order.PackageSize,
            EstimatedDistanceKm = order.EstimatedDistanceKm,
            EstimatedDuration = order.EstimatedDuration,
            Price = order.Price
        };

        var orderDto = new OrderDto(
            Id: assignedOrder.Id,
            CustomerId: assignedOrder.CustomerId,
            CourierId: assignedOrder.CourierId,
            TrackingNumber: assignedOrder.TrackingNumber,
            PickupAddress: assignedOrder.PickupAddress,
            PickupLatitude: assignedOrder.PickupLatitude,
            PickupLongitude: assignedOrder.PickupLongitude,
            DeliveryAddress: assignedOrder.DeliveryAddress,
            DeliveryLatitude: assignedOrder.DeliveryLatitude,
            DeliveryLongitude: assignedOrder.DeliveryLongitude,
            PackageDescription: null,
            PackageWeight: assignedOrder.PackageWeight,
            PackageSize: assignedOrder.PackageSize,
            EstimatedDistanceKm: assignedOrder.EstimatedDistanceKm,
            EstimatedDuration: assignedOrder.EstimatedDuration,
            Price: assignedOrder.Price,
            Status: assignedOrder.Status,
            CreatedAt: DateTime.UtcNow,
            PickedUpAt: null,
            DeliveredAt: null
        );

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _mockCourierRepository
            .Setup(x => x.GetByIdAsync(courierId))
            .ReturnsAsync(courier);

        _mockOrderRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Order>()))
            .Returns<Order>(o => Task.FromResult(o));

        _mockCourierRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Courier>()))
            .Returns<Courier>(c => Task.FromResult(c));

        _mockOrderRepository
            .Setup(x => x.AddStatusHistoryAsync(It.IsAny<OrderStatusHistory>()))
            .Returns(Task.CompletedTask);

        _mockTrackingHubService
            .Setup(x => x.NotifyNewOrderAssignedAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        _mockMapper
            .Setup(x => x.Map<OrderDto>(It.IsAny<Order>()))
            .Returns(orderDto);

        var result = await _orderAssignmentService.AssignOrderAsync(orderId, courierId);

        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Assigned);
        result.CourierId.Should().Be(courierId);
        _mockOrderRepository.Verify(x => x.AddStatusHistoryAsync(It.IsAny<OrderStatusHistory>()), Times.Once);
        _mockTrackingHubService.Verify(x => x.NotifyNewOrderAssignedAsync(courierId, orderId), Times.Once);
    }

    #endregion

    #region AutoAssignOrderAsync Tests

    [Fact]
    public async Task AutoAssignOrderAsync_ShouldMoveCreatedOrderToPending_WhenNoCouriersAvailable()
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
            PackageWeight = 5m,
            PackageSize = PackageSize.Small,
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
            PackageWeight: order.PackageWeight,
            PackageSize: order.PackageSize,
            EstimatedDistanceKm: order.EstimatedDistanceKm,
            EstimatedDuration: order.EstimatedDuration,
            Price: order.Price,
            Status: OrderStatus.Pending,
            CreatedAt: DateTime.UtcNow,
            PickedUpAt: null,
            DeliveredAt: null
        );

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _mockCourierRepository
            .Setup(x => x.GetAvailableAsync())
            .ReturnsAsync(new List<Courier>());

        _mockOrderRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Order>()))
            .Returns<Order>(o => Task.FromResult(o));

        _mockOrderRepository
            .Setup(x => x.AddStatusHistoryAsync(It.IsAny<OrderStatusHistory>()))
            .Returns(Task.CompletedTask);

        _mockMapper
            .Setup(x => x.Map<OrderDto>(It.IsAny<Order>()))
            .Returns(orderDto);

        var result = await _orderAssignmentService.AutoAssignOrderAsync(orderId);

        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Pending);
        _mockOrderRepository.Verify(x => x.UpdateAsync(It.IsAny<Order>()), Times.Once);
        _mockOrderRepository.Verify(x => x.AddStatusHistoryAsync(It.IsAny<OrderStatusHistory>()), Times.Once);
    }

    [Fact]
    public async Task AutoAssignOrderAsync_ShouldNotThrow_WhenNoCouriersAvailableForCreatedOrder()
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
            PackageWeight = 5m,
            PackageSize = PackageSize.Small,
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
            PackageWeight: order.PackageWeight,
            PackageSize: order.PackageSize,
            EstimatedDistanceKm: order.EstimatedDistanceKm,
            EstimatedDuration: order.EstimatedDuration,
            Price: order.Price,
            Status: OrderStatus.Pending,
            CreatedAt: DateTime.UtcNow,
            PickedUpAt: null,
            DeliveredAt: null
        );

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _mockCourierRepository
            .Setup(x => x.GetAvailableAsync())
            .ReturnsAsync(new List<Courier>());

        _mockOrderRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Order>()))
            .Returns<Order>(o => Task.FromResult(o));

        _mockMapper
            .Setup(x => x.Map<OrderDto>(It.IsAny<Order>()))
            .Returns(orderDto);

        var act = async () => await _orderAssignmentService.AutoAssignOrderAsync(orderId);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task AutoAssignOrderAsync_ShouldAssignToNearestCourier_WhenCouriersAvailable()
    {
        var orderId = Guid.NewGuid();
        var courierId = Guid.NewGuid();
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
            PackageWeight = 5m,
            PackageSize = PackageSize.Small,
            EstimatedDistanceKm = 5m,
            EstimatedDuration = "0h 8m",
            Price = 15m
        };

        var courier = new Courier
        {
            Id = courierId,
            UserId = Guid.NewGuid(),
            VehicleType = VehicleType.Motorcycle,
            CurrentLatitude = 40.7400,
            CurrentLongitude = -73.9700,
            IsAvailable = true,
            Rating = 4.5m,
            TotalDeliveries = 100
        };

        var assignedOrder = new Order
        {
            Id = orderId,
            CustomerId = order.CustomerId,
            CourierId = courierId,
            Status = OrderStatus.Assigned,
            TrackingNumber = order.TrackingNumber,
            PickupAddress = order.PickupAddress,
            PickupLatitude = order.PickupLatitude,
            PickupLongitude = order.PickupLongitude,
            DeliveryAddress = order.DeliveryAddress,
            DeliveryLatitude = order.DeliveryLatitude,
            DeliveryLongitude = order.DeliveryLongitude,
            PackageWeight = order.PackageWeight,
            PackageSize = order.PackageSize,
            EstimatedDistanceKm = order.EstimatedDistanceKm,
            EstimatedDuration = order.EstimatedDuration,
            Price = order.Price
        };

        var orderDto = new OrderDto(
            Id: assignedOrder.Id,
            CustomerId: assignedOrder.CustomerId,
            CourierId: assignedOrder.CourierId,
            TrackingNumber: assignedOrder.TrackingNumber,
            PickupAddress: assignedOrder.PickupAddress,
            PickupLatitude: assignedOrder.PickupLatitude,
            PickupLongitude: assignedOrder.PickupLongitude,
            DeliveryAddress: assignedOrder.DeliveryAddress,
            DeliveryLatitude: assignedOrder.DeliveryLatitude,
            DeliveryLongitude: assignedOrder.DeliveryLongitude,
            PackageDescription: null,
            PackageWeight: assignedOrder.PackageWeight,
            PackageSize: assignedOrder.PackageSize,
            EstimatedDistanceKm: assignedOrder.EstimatedDistanceKm,
            EstimatedDuration: assignedOrder.EstimatedDuration,
            Price: assignedOrder.Price,
            Status: assignedOrder.Status,
            CreatedAt: DateTime.UtcNow,
            PickedUpAt: null,
            DeliveredAt: null
        );

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _mockCourierRepository
            .Setup(x => x.GetAvailableAsync())
            .ReturnsAsync(new List<Courier> { courier });

        _mockCourierRepository
            .Setup(x => x.GetByIdAsync(courierId))
            .ReturnsAsync(courier);

        _mockOrderRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Order>()))
            .Returns<Order>(o => Task.FromResult(o));

        _mockCourierRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Courier>()))
            .Returns<Courier>(c => Task.FromResult(c));

        _mockOrderRepository
            .Setup(x => x.AddStatusHistoryAsync(It.IsAny<OrderStatusHistory>()))
            .Returns(Task.CompletedTask);

        _mockTrackingHubService
            .Setup(x => x.NotifyNewOrderAssignedAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        _mockMapper
            .Setup(x => x.Map<OrderDto>(It.IsAny<Order>()))
            .Returns(orderDto);

        var result = await _orderAssignmentService.AutoAssignOrderAsync(orderId);

        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Assigned);
        result.CourierId.Should().Be(courierId);
        _mockOrderRepository.Verify(x => x.AddStatusHistoryAsync(It.IsAny<OrderStatusHistory>()), Times.Once);
    }

    [Fact]
    public async Task AutoAssignOrderAsync_ShouldThrow_WhenOrderNotFound()
    {
        var orderId = Guid.NewGuid();

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync((Order?)null);

        var act = async () => await _orderAssignmentService.AutoAssignOrderAsync(orderId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AutoAssignOrderAsync_ShouldThrow_WhenOrderInInvalidStatus()
    {
        var orderId = Guid.NewGuid();
        var order = new Order
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
            Price = 15m
        };

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        var act = async () => await _orderAssignmentService.AutoAssignOrderAsync(orderId);

        await act.Should().ThrowAsync<Domain.Exceptions.InvalidOperationException>();
    }

    #endregion

    #region UnassignOrderAsync Tests

    [Fact]
    public async Task UnassignOrderAsync_ShouldMoveAssignedOrderToPending()
    {
        var orderId = Guid.NewGuid();
        var courierId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            CourierId = courierId,
            Status = OrderStatus.Assigned,
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

        var courier = new Courier
        {
            Id = courierId,
            UserId = Guid.NewGuid(),
            VehicleType = VehicleType.Motorcycle,
            CurrentLatitude = 40.7400,
            CurrentLongitude = -73.9700,
            IsAvailable = false,
            Rating = 4.5m,
            TotalDeliveries = 100
        };

        var unassignedOrder = new Order
        {
            Id = orderId,
            CustomerId = order.CustomerId,
            CourierId = null,
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
            Id: unassignedOrder.Id,
            CustomerId: unassignedOrder.CustomerId,
            CourierId: null,
            TrackingNumber: unassignedOrder.TrackingNumber,
            PickupAddress: unassignedOrder.PickupAddress,
            PickupLatitude: unassignedOrder.PickupLatitude,
            PickupLongitude: unassignedOrder.PickupLongitude,
            DeliveryAddress: unassignedOrder.DeliveryAddress,
            DeliveryLatitude: unassignedOrder.DeliveryLatitude,
            DeliveryLongitude: unassignedOrder.DeliveryLongitude,
            PackageDescription: null,
            PackageWeight: 0m,
            PackageSize: PackageSize.Small,
            EstimatedDistanceKm: unassignedOrder.EstimatedDistanceKm,
            EstimatedDuration: unassignedOrder.EstimatedDuration,
            Price: unassignedOrder.Price,
            Status: unassignedOrder.Status,
            CreatedAt: DateTime.UtcNow,
            PickedUpAt: null,
            DeliveredAt: null
        );

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _mockCourierRepository
            .Setup(x => x.GetByIdAsync(courierId))
            .ReturnsAsync(courier);

        _mockOrderRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Order>()))
            .Returns<Order>(o => Task.FromResult(o));

        _mockCourierRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Courier>()))
            .Returns<Courier>(c => Task.FromResult(c));

        _mockOrderRepository
            .Setup(x => x.AddStatusHistoryAsync(It.IsAny<OrderStatusHistory>()))
            .Returns(Task.CompletedTask);

        _mockMapper
            .Setup(x => x.Map<OrderDto>(It.IsAny<Order>()))
            .Returns(orderDto);

        var result = await _orderAssignmentService.UnassignOrderAsync(orderId);

        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Pending);
        result.CourierId.Should().BeNull();
        _mockOrderRepository.Verify(x => x.UpdateAsync(It.IsAny<Order>()), Times.Once);
        _mockCourierRepository.Verify(x => x.UpdateAsync(It.IsAny<Courier>()), Times.Once);
        _mockOrderRepository.Verify(x => x.AddStatusHistoryAsync(It.IsAny<OrderStatusHistory>()), Times.Once);
    }

    [Fact]
    public async Task UnassignOrderAsync_ShouldThrow_WhenOrderNotAssigned()
    {
        var orderId = Guid.NewGuid();
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

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        var act = async () => await _orderAssignmentService.UnassignOrderAsync(orderId);

        await act.Should().ThrowAsync<Domain.Exceptions.InvalidOperationException>();
    }

    #endregion
}
