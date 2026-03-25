using AutoMapper;
using CourierTrack.Application.DTOs;
using CourierTrack.Domain.Entities;

namespace CourierTrack.Application.Mappers;

public class OrderProfiler : Profile
{
    public OrderProfiler()
    {
        CreateMap<Order, OrderDto>();

        CreateMap<CreateOrderDto, Order>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CourierId, opt => opt.Ignore())
            .ForMember(dest => dest.TrackingNumber, opt => opt.Ignore())
            .ForMember(dest => dest.EstimatedDistanceKm, opt => opt.Ignore())
            .ForMember(dest => dest.EstimatedDuration, opt => opt.Ignore())
            .ForMember(dest => dest.Price, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.PickedUpAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeliveredAt, opt => opt.Ignore())
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForMember(dest => dest.Courier, opt => opt.Ignore());

        CreateMap<UpdateOrderDto, Order>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
            .ForMember(dest => dest.TrackingNumber, opt => opt.Ignore())
            .ForMember(dest => dest.PickupAddress, opt => opt.Ignore())
            .ForMember(dest => dest.PickupLatitude, opt => opt.Ignore())
            .ForMember(dest => dest.PickupLongitude, opt => opt.Ignore())
            .ForMember(dest => dest.DeliveryAddress, opt => opt.Ignore())
            .ForMember(dest => dest.DeliveryLatitude, opt => opt.Ignore())
            .ForMember(dest => dest.DeliveryLongitude, opt => opt.Ignore())
            .ForMember(dest => dest.PackageDescription, opt => opt.Ignore())
            .ForMember(dest => dest.PackageWeight, opt => opt.Ignore())
            .ForMember(dest => dest.PackageSize, opt => opt.Ignore())
            .ForMember(dest => dest.EstimatedDistanceKm, opt => opt.Ignore())
            .ForMember(dest => dest.EstimatedDuration, opt => opt.Ignore())
            .ForMember(dest => dest.Price, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForMember(dest => dest.Courier, opt => opt.Ignore());
    }
}
