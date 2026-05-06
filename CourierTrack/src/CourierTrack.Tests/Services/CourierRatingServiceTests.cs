using AutoMapper;
using CourierTrack.Application.DTOs;
using CourierTrack.Application.Interfaces.Repositories;
using CourierTrack.Application.Interfaces.Services;
using CourierTrack.Application.Services;
using CourierTrack.Domain.Entities;
using CourierTrack.Domain.Enums;
using CourierTrack.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace CourierTrack.Tests.Services;

public class CourierRatingServiceTests
{
    private readonly Mock<ICourierRatingRepository> _mockCourierRatingRepository;
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<ICourierRepository> _mockCourierRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly ICourierRatingService _courierRatingService;

    public CourierRatingServiceTests()
    {
        _mockCourierRatingRepository = new Mock<ICourierRatingRepository>();
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockCourierRepository = new Mock<ICourierRepository>();
        _mockMapper = new Mock<IMapper>();

        _courierRatingService = new CourierRatingService(
            _mockCourierRatingRepository.Object,
            _mockOrderRepository.Object,
            _mockCourierRepository.Object,
            _mockMapper.Object
        );
    }

    #region CreateRatingAsync Tests

    [Fact]
    public async Task CreateRatingAsync_ShouldThrow_WhenRatingIsLessThanOne()
    {
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var rateCourierDto = new RateCourierDto(Rating: 0, Feedback: "Bad service");

        await Assert.ThrowsAsync<Domain.Exceptions.InvalidOperationException>(
            () => _courierRatingService.CreateRatingAsync(orderId, rateCourierDto, customerId)
        );
    }

    [Fact]
    public async Task CreateRatingAsync_ShouldThrow_WhenRatingIsGreaterThanFive()
    {
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var rateCourierDto = new RateCourierDto(Rating: 6, Feedback: "Excellent service");

        await Assert.ThrowsAsync<Domain.Exceptions.InvalidOperationException>(
            () => _courierRatingService.CreateRatingAsync(orderId, rateCourierDto, customerId)
        );
    }

    [Fact]
    public async Task CreateRatingAsync_ShouldThrow_WhenOrderNotFound()
    {
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var rateCourierDto = new RateCourierDto(Rating: 4, Feedback: "Good service");

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync((Order?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _courierRatingService.CreateRatingAsync(orderId, rateCourierDto, customerId)
        );
    }

    [Fact]
    public async Task CreateRatingAsync_ShouldThrow_WhenCustomerIsNotOrderOwner()
    {
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var differentCustomerId = Guid.NewGuid();
        var courierId = Guid.NewGuid();

        var order = new Order
        {
            Id = orderId,
            CustomerId = customerId,
            CourierId = courierId,
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

        var rateCourierDto = new RateCourierDto(Rating: 4, Feedback: "Good service");

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        await Assert.ThrowsAsync<Domain.Exceptions.InvalidOperationException>(
            () => _courierRatingService.CreateRatingAsync(orderId, rateCourierDto, differentCustomerId)
        );
    }

    [Fact]
    public async Task CreateRatingAsync_ShouldThrow_WhenOrderNotDelivered()
    {
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var courierId = Guid.NewGuid();

        var order = new Order
        {
            Id = orderId,
            CustomerId = customerId,
            CourierId = courierId,
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
            Price = 15m
        };

        var rateCourierDto = new RateCourierDto(Rating: 4, Feedback: "Good service");

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        await Assert.ThrowsAsync<Domain.Exceptions.InvalidOperationException>(
            () => _courierRatingService.CreateRatingAsync(orderId, rateCourierDto, customerId)
        );
    }

    [Fact]
    public async Task CreateRatingAsync_ShouldThrow_WhenOrderHasNoCourier()
    {
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var order = new Order
        {
            Id = orderId,
            CustomerId = customerId,
            CourierId = null,
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

        var rateCourierDto = new RateCourierDto(Rating: 4, Feedback: "Good service");

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        await Assert.ThrowsAsync<Domain.Exceptions.InvalidOperationException>(
            () => _courierRatingService.CreateRatingAsync(orderId, rateCourierDto, customerId)
        );
    }

    [Fact]
    public async Task CreateRatingAsync_ShouldThrow_WhenOrderAlreadyRated()
    {
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var courierId = Guid.NewGuid();

        var order = new Order
        {
            Id = orderId,
            CustomerId = customerId,
            CourierId = courierId,
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

        var existingRating = new CourierRating
        {
            Id = 1,
            OrderId = orderId,
            CourierId = courierId,
            CustomerId = customerId,
            Score = 5,
            Comment = "Excellent",
            CreatedAt = DateTime.UtcNow
        };

        var rateCourierDto = new RateCourierDto(Rating: 4, Feedback: "Good service");

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _mockCourierRatingRepository
            .Setup(x => x.GetByOrderIdAsync(orderId))
            .ReturnsAsync(existingRating);

        await Assert.ThrowsAsync<Domain.Exceptions.InvalidOperationException>(
            () => _courierRatingService.CreateRatingAsync(orderId, rateCourierDto, customerId)
        );
    }

    [Fact]
    public async Task CreateRatingAsync_ShouldThrow_WhenCourierNotFound()
    {
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var courierId = Guid.NewGuid();

        var order = new Order
        {
            Id = orderId,
            CustomerId = customerId,
            CourierId = courierId,
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

        var rateCourierDto = new RateCourierDto(Rating: 4, Feedback: "Good service");

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _mockCourierRatingRepository
            .Setup(x => x.GetByOrderIdAsync(orderId))
            .ReturnsAsync((CourierRating?)null);

        _mockCourierRepository
            .Setup(x => x.GetByIdAsync(courierId))
            .ReturnsAsync((Courier?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _courierRatingService.CreateRatingAsync(orderId, rateCourierDto, customerId)
        );
    }

    [Fact]
    public async Task CreateRatingAsync_ShouldCreateRating_WithValidData()
    {
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var courierId = Guid.NewGuid();
        var rating = 5;
        var feedback = "Excellent service!";

        var order = new Order
        {
            Id = orderId,
            CustomerId = customerId,
            CourierId = courierId,
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

        var courier = new Courier
        {
            Id = courierId,
            UserId = Guid.NewGuid(),
            VehicleType = VehicleType.Motorcycle,
            IsAvailable = true,
            Rating = 4.5m,
            TotalDeliveries = 10
        };

        var createdRating = new CourierRating
        {
            Id = 1,
            OrderId = orderId,
            CourierId = courierId,
            CustomerId = customerId,
            Score = rating,
            Comment = feedback,
            CreatedAt = DateTime.UtcNow
        };

        var rateCourierDto = new RateCourierDto(Rating: rating, Feedback: feedback);

        var ratingDto = new CourierRatingDto(
            Id: 1,
            OrderId: orderId,
            CourierId: courierId,
            CustomerId: customerId,
            Score: rating,
            Comment: feedback,
            CreatedAt: createdRating.CreatedAt
        );

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _mockCourierRatingRepository
            .Setup(x => x.GetByOrderIdAsync(orderId))
            .ReturnsAsync((CourierRating?)null);

        _mockCourierRepository
            .Setup(x => x.GetByIdAsync(courierId))
            .ReturnsAsync(courier);

        _mockCourierRatingRepository
            .Setup(x => x.AddAsync(It.IsAny<CourierRating>()))
            .ReturnsAsync(createdRating);

        _mockCourierRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Courier>()))
            .Returns<Courier>(c => Task.FromResult(c));

        _mockMapper
            .Setup(x => x.Map<CourierRatingDto>(createdRating))
            .Returns(ratingDto);

        var result = await _courierRatingService.CreateRatingAsync(orderId, rateCourierDto, customerId);

        result.Should().NotBeNull();
        result.Score.Should().Be(rating);
        result.Comment.Should().Be(feedback);
        result.CourierId.Should().Be(courierId);
        result.OrderId.Should().Be(orderId);
        _mockCourierRepository.Verify(x => x.UpdateAsync(It.IsAny<Courier>()), Times.Once);
    }

    [Fact]
    public async Task CreateRatingAsync_ShouldUpdateCourierAverageRating()
    {
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var courierId = Guid.NewGuid();

        var order = new Order
        {
            Id = orderId,
            CustomerId = customerId,
            CourierId = courierId,
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

        var courier = new Courier
        {
            Id = courierId,
            UserId = Guid.NewGuid(),
            VehicleType = VehicleType.Motorcycle,
            IsAvailable = true,
            Rating = 4m,
            TotalDeliveries = 4
        };

        var createdRating = new CourierRating
        {
            Id = 1,
            OrderId = orderId,
            CourierId = courierId,
            CustomerId = customerId,
            Score = 5,
            Comment = "Great!",
            CreatedAt = DateTime.UtcNow
        };

        var rateCourierDto = new RateCourierDto(Rating: 5, Feedback: "Great!");

        var ratingDto = new CourierRatingDto(
            Id: 1,
            OrderId: orderId,
            CourierId: courierId,
            CustomerId: customerId,
            Score: 5,
            Comment: "Great!",
            CreatedAt: createdRating.CreatedAt
        );

        _mockOrderRepository
            .Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _mockCourierRatingRepository
            .Setup(x => x.GetByOrderIdAsync(orderId))
            .ReturnsAsync((CourierRating?)null);

        _mockCourierRepository
            .Setup(x => x.GetByIdAsync(courierId))
            .ReturnsAsync(courier);

        _mockCourierRatingRepository
            .Setup(x => x.AddAsync(It.IsAny<CourierRating>()))
            .ReturnsAsync(createdRating);

        _mockCourierRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Courier>()))
            .Callback<Courier>(c =>
            {
                c.TotalDeliveries.Should().Be(5);
                c.Rating.Should().Be(4.2m);
            })
            .Returns<Courier>(c => Task.FromResult(c));

        _mockMapper
            .Setup(x => x.Map<CourierRatingDto>(createdRating))
            .Returns(ratingDto);

        await _courierRatingService.CreateRatingAsync(orderId, rateCourierDto, customerId);

        _mockCourierRepository.Verify(x => x.UpdateAsync(It.IsAny<Courier>()), Times.Once);
    }

    #endregion

    #region GetRatingByOrderIdAsync Tests

    [Fact]
    public async Task GetRatingByOrderIdAsync_ShouldThrow_WhenRatingNotFound()
    {
        var orderId = Guid.NewGuid();

        _mockCourierRatingRepository
            .Setup(x => x.GetByOrderIdAsync(orderId))
            .ReturnsAsync((CourierRating?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _courierRatingService.GetRatingByOrderIdAsync(orderId)
        );
    }

    [Fact]
    public async Task GetRatingByOrderIdAsync_ShouldReturnRating_WhenFound()
    {
        var orderId = Guid.NewGuid();
        var courierId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var rating = new CourierRating
        {
            Id = 1,
            OrderId = orderId,
            CourierId = courierId,
            CustomerId = customerId,
            Score = 5,
            Comment = "Excellent",
            CreatedAt = DateTime.UtcNow
        };

        var ratingDto = new CourierRatingDto(
            Id: 1,
            OrderId: orderId,
            CourierId: courierId,
            CustomerId: customerId,
            Score: 5,
            Comment: "Excellent",
            CreatedAt: rating.CreatedAt
        );

        _mockCourierRatingRepository
            .Setup(x => x.GetByOrderIdAsync(orderId))
            .ReturnsAsync(rating);

        _mockMapper
            .Setup(x => x.Map<CourierRatingDto>(rating))
            .Returns(ratingDto);

        var result = await _courierRatingService.GetRatingByOrderIdAsync(orderId);

        result.Should().NotBeNull();
        result.OrderId.Should().Be(orderId);
        result.Score.Should().Be(5);
        result.Comment.Should().Be("Excellent");
    }

    #endregion

    #region GetCourierRatingsAsync Tests

    [Fact]
    public async Task GetCourierRatingsAsync_ShouldThrow_WhenCourierNotFound()
    {
        var courierId = Guid.NewGuid();

        _mockCourierRepository
            .Setup(x => x.GetByIdAsync(courierId))
            .ReturnsAsync((Courier?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _courierRatingService.GetCourierRatingsAsync(courierId)
        );
    }

    [Fact]
    public async Task GetCourierRatingsAsync_ShouldReturnEmptyList_WhenCourierHasNoRatings()
    {
        var courierId = Guid.NewGuid();

        var courier = new Courier
        {
            Id = courierId,
            UserId = Guid.NewGuid(),
            VehicleType = VehicleType.Motorcycle,
            IsAvailable = true,
            Rating = null,
            TotalDeliveries = 0
        };

        _mockCourierRepository
            .Setup(x => x.GetByIdAsync(courierId))
            .ReturnsAsync(courier);

        _mockCourierRatingRepository
            .Setup(x => x.GetByCourierIdAsync(courierId))
            .ReturnsAsync(new List<CourierRating>());

        _mockMapper
            .Setup(x => x.Map<IEnumerable<CourierRatingDto>>(It.IsAny<List<CourierRating>>()))
            .Returns(new List<CourierRatingDto>());

        var result = await _courierRatingService.GetCourierRatingsAsync(courierId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCourierRatingsAsync_ShouldReturnAllRatings_ForCourier()
    {
        var courierId = Guid.NewGuid();
        var customerId1 = Guid.NewGuid();
        var customerId2 = Guid.NewGuid();

        var courier = new Courier
        {
            Id = courierId,
            UserId = Guid.NewGuid(),
            VehicleType = VehicleType.Motorcycle,
            IsAvailable = true,
            Rating = 4.5m,
            TotalDeliveries = 10
        };

        var ratings = new List<CourierRating>
        {
            new()
            {
                Id = 1,
                OrderId = Guid.NewGuid(),
                CourierId = courierId,
                CustomerId = customerId1,
                Score = 5,
                Comment = "Great",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                Id = 2,
                OrderId = Guid.NewGuid(),
                CourierId = courierId,
                CustomerId = customerId2,
                Score = 4,
                Comment = "Good",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        var ratingDtos = new List<CourierRatingDto>
        {
            new(1, ratings[0].OrderId, courierId, customerId1, 5, "Great", ratings[0].CreatedAt),
            new(2, ratings[1].OrderId, courierId, customerId2, 4, "Good", ratings[1].CreatedAt)
        };

        _mockCourierRepository
            .Setup(x => x.GetByIdAsync(courierId))
            .ReturnsAsync(courier);

        _mockCourierRatingRepository
            .Setup(x => x.GetByCourierIdAsync(courierId))
            .ReturnsAsync(ratings);

        _mockMapper
            .Setup(x => x.Map<IEnumerable<CourierRatingDto>>(ratings))
            .Returns(ratingDtos);

        var result = await _courierRatingService.GetCourierRatingsAsync(courierId);

        result.Should().NotBeEmpty();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(r => r.CourierId.Should().Be(courierId));
    }

    #endregion

    #region GetCustomerRatingsAsync Tests

    [Fact]
    public async Task GetCustomerRatingsAsync_ShouldReturnEmptyList_WhenCustomerHasNoRatings()
    {
        var customerId = Guid.NewGuid();

        _mockCourierRatingRepository
            .Setup(x => x.GetByCustomerIdAsync(customerId))
            .ReturnsAsync(new List<CourierRating>());

        _mockMapper
            .Setup(x => x.Map<IEnumerable<CourierRatingDto>>(It.IsAny<List<CourierRating>>()))
            .Returns(new List<CourierRatingDto>());

        var result = await _courierRatingService.GetCustomerRatingsAsync(customerId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCustomerRatingsAsync_ShouldReturnAllRatings_FromCustomer()
    {
        var customerId = Guid.NewGuid();
        var courierId1 = Guid.NewGuid();
        var courierId2 = Guid.NewGuid();

        var ratings = new List<CourierRating>
        {
            new()
            {
                Id = 1,
                OrderId = Guid.NewGuid(),
                CourierId = courierId1,
                CustomerId = customerId,
                Score = 5,
                Comment = "Excellent",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                Id = 2,
                OrderId = Guid.NewGuid(),
                CourierId = courierId2,
                CustomerId = customerId,
                Score = 3,
                Comment = "Average",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        var ratingDtos = new List<CourierRatingDto>
        {
            new(1, ratings[0].OrderId, courierId1, customerId, 5, "Excellent", ratings[0].CreatedAt),
            new(2, ratings[1].OrderId, courierId2, customerId, 3, "Average", ratings[1].CreatedAt)
        };

        _mockCourierRatingRepository
            .Setup(x => x.GetByCustomerIdAsync(customerId))
            .ReturnsAsync(ratings);

        _mockMapper
            .Setup(x => x.Map<IEnumerable<CourierRatingDto>>(ratings))
            .Returns(ratingDtos);

        var result = await _courierRatingService.GetCustomerRatingsAsync(customerId);

        result.Should().NotBeEmpty();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(r => r.CustomerId.Should().Be(customerId));
    }

    #endregion

    #region GetCourierAverageRatingAsync Tests

    [Fact]
    public async Task GetCourierAverageRatingAsync_ShouldThrow_WhenCourierNotFound()
    {
        var courierId = Guid.NewGuid();

        _mockCourierRepository
            .Setup(x => x.GetByIdAsync(courierId))
            .ReturnsAsync((Courier?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _courierRatingService.GetCourierAverageRatingAsync(courierId)
        );
    }

    [Fact]
    public async Task GetCourierAverageRatingAsync_ShouldReturnNull_WhenCourierHasNoRating()
    {
        var courierId = Guid.NewGuid();

        var courier = new Courier
        {
            Id = courierId,
            UserId = Guid.NewGuid(),
            VehicleType = VehicleType.Motorcycle,
            IsAvailable = true,
            Rating = null,
            TotalDeliveries = 0
        };

        _mockCourierRepository
            .Setup(x => x.GetByIdAsync(courierId))
            .ReturnsAsync(courier);

        var result = await _courierRatingService.GetCourierAverageRatingAsync(courierId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCourierAverageRatingAsync_ShouldReturnAverageRating_WhenCourierExists()
    {
        var courierId = Guid.NewGuid();
        var expectedRating = 4.5m;

        var courier = new Courier
        {
            Id = courierId,
            UserId = Guid.NewGuid(),
            VehicleType = VehicleType.Car,
            IsAvailable = true,
            Rating = expectedRating,
            TotalDeliveries = 10
        };

        _mockCourierRepository
            .Setup(x => x.GetByIdAsync(courierId))
            .ReturnsAsync(courier);

        var result = await _courierRatingService.GetCourierAverageRatingAsync(courierId);

        result.Should().Be(expectedRating);
    }

    #endregion
}
