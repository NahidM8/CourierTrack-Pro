namespace CourierTrack.Domain.Constants;

public static class ApplicationConstants
{
    public static class Order
    {
        public const string TrackingNumberPrefix = "CT";
        public const decimal AverageSpeedKmPerHour = 35m;
        public const int TrackingNumberRandomLength = 8;
        public const string CancellationReason = "Order cancelled by user.";
    }

    public static class Rating
    {
        public const int MinimumRating = 1;
        public const int MaximumRating = 5;
    }

    public static class Jwt
    {
        public const string ConfigurationSection = "Jwt";
        public const string IssuerKey = "Issuer";
        public const string AudienceKey = "Audience";
        public const string KeyProperty = "SecretKey";
        public const string AccessToken = "access_token";
    }

    public static class Hubs
    {
        public const string TrackingHubPath = "/hubs/tracking";
        public const string OrderStatusUpdated = "OrderStatusUpdated";
        public const string CourierLocationUpdated = "CourierLocationUpdated";
        public const string NewOrderAssigned = "NewOrderAssigned";
        public const string OrderPickedUp = "OrderPickedUp";
    }

    public static class Headers
    {
        public const string Authorization = "Authorization";
    }

    public static class Pagination
    {
        public const int DefaultPage = 1;
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 50;
    }

    public static class Pricing
    {
        public const string ConfigurationSection = "Pricing";
    }

    public static class Assignment
    {
        public const string ConfigurationSection = "Assignment";
        public const decimal HeavyPackageWeightThreshold = 20m;
    }

    public static class Groups
    {
        public const string OrderGroupPrefix = "order-";
        public const string CourierGroupPrefix = "courier-";
        public const string AdminGroup = "admins";
    }
}
