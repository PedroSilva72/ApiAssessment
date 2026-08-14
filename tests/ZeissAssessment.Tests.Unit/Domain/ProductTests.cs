using FluentAssertions;
using ZeissAssessment.Domain.Entities;
using Xunit;

namespace ZeissAssessment.Tests.Unit.Domain;

public class ProductTests
{
    [Fact]
    public void DecrementStock_ReducesStock_WhenEnoughAvailable()
    {
        var product = new Product { Stock = 10 };
        product.DecrementStock(3);
        product.Stock.Should().Be(7);
    }

    [Fact]
    public void DecrementStock_Throws_WhenInsufficientStock()
    {
        var product = new Product { Stock = 2 };
        var act = () => product.DecrementStock(5);
        act.Should().Throw<InvalidStockOperationException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DecrementStock_Throws_OnNonPositiveQuantity(int quantity)
    {
        var product = new Product { Stock = 10 };
        var act = () => product.DecrementStock(quantity);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void IncrementStock_IncreasesStock()
    {
        var product = new Product { Stock = 4 };
        product.IncrementStock(6);
        product.Stock.Should().Be(10);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void IncrementStock_Throws_OnNonPositiveQuantity(int quantity)
    {
        var product = new Product { Stock = 4 };
        var act = () => product.IncrementStock(quantity);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
