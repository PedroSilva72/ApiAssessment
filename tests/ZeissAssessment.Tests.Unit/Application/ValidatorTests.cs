using FluentAssertions;
using Xunit;
using ZeissAssessment.Application.Products;

namespace ZeissAssessment.Tests.Unit.Application;

public class ValidatorTests
{
    [Fact]
    public void CreateRequest_Invalid_WhenNameEmpty()
    {
        var validator = new CreateProductRequestValidator();
        var result = validator.Validate(new CreateProductRequest("", null, 1m, 1));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateRequest_Invalid_WhenPriceNegative()
    {
        var validator = new CreateProductRequestValidator();
        var result = validator.Validate(new CreateProductRequest("A", null, -1m, 1));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateRequest_Valid()
    {
        var validator = new CreateProductRequestValidator();
        var result = validator.Validate(new CreateProductRequest("A", null, 0m, 0));
        result.IsValid.Should().BeTrue();
    }
}
