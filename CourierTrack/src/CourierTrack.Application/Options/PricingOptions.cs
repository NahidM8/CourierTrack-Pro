namespace CourierTrack.Application.Options;

public sealed class PricingOptions
{
    public const string SectionName = "Pricing";
    public decimal PricePerKm { get; init; }
    public decimal WeightMultiplier { get; init; }
    public decimal MinimumPrice { get; init; }
    public decimal SmallPackageCharge { get; init; }
    public decimal MediumPackageCharge { get; init; }
    public decimal LargePackageCharge { get; init; }
    public decimal XLargePackageCharge { get; init; }
}
