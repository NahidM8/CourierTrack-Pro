namespace CourierTrack.Application.DTOs;

public record LocationUpdateMessage(
    Guid CourierId,
    double Latitude,
    double Longitude,
    LocationPoint CurrentLocation);

public record OrderLocationUpdateMessage(
    Guid OrderId,
    Guid CourierId,
    LocationPoint CurrentLocation);

public record OrderStatusUpdateMessage(
    Guid OrderId,
    string Status);

public record NewOrderAssignedMessage(
    Guid CourierId,
    Guid OrderId);

public record OrderPickedUpMessage(
    Guid OrderId);

public record LocationPoint(
    double Latitude,
    double Longitude);
