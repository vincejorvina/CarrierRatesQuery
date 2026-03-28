using CarrierRatesQuery.Api.Data.Entities;

namespace CarrierRatesQuery.Tests.Entities;

public class CarrierTests
{
    [Theory]
    [InlineData("FedEx", "fedex")]
    [InlineData("UPS 2nd Day Air", "ups2nddayair")]
    [InlineData("DHL-Express Worldwide!", "dhlexpressworldwide")]
    [InlineData("  abc_123  ", "abc123")]
    [InlineData("!!!", "")]
    public void Slug_NameContainsNonAlphanumericCharacters_ReturnsLowercaseAlphanumericOnly(string name, string expectedSlug)
    {
        // Arrange
        var carrier = new Carrier
        {
            Name = name
        };

        // Act
        var actualSlug = carrier.Slug;

        // Assert
        Assert.Equal(expectedSlug, actualSlug);
        Assert.Matches("^[a-z0-9]*$", actualSlug);
    }
}
