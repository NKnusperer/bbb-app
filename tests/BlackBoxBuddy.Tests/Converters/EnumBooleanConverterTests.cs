using System.Globalization;
using BlackBoxBuddy.Converters;
using BlackBoxBuddy.Models.Settings;
using FluentAssertions;

namespace BlackBoxBuddy.Tests.Converters;

public class EnumBooleanConverterTests
{
    private readonly EnumBooleanConverter _sut = EnumBooleanConverter.Instance;

    [Fact]
    public void Convert_WhenValueEqualsParameter_ReturnsTrue()
    {
        var result = _sut.Convert(WifiBand.FiveGHz, typeof(bool), WifiBand.FiveGHz, CultureInfo.InvariantCulture);
        result.Should().Be(true);
    }

    [Fact]
    public void Convert_WhenValueDoesNotEqualParameter_ReturnsFalse()
    {
        var result = _sut.Convert(WifiBand.FiveGHz, typeof(bool), WifiBand.TwoPointFourGHz, CultureInfo.InvariantCulture);
        result.Should().Be(false);
    }

    [Fact]
    public void Convert_WhenValueIsNull_ReturnsFalse()
    {
        var result = _sut.Convert(null, typeof(bool), WifiBand.FiveGHz, CultureInfo.InvariantCulture);
        result.Should().Be(false);
    }

    [Fact]
    public void ConvertBack_WhenValueIsTrue_ReturnsParameter()
    {
        var result = _sut.ConvertBack(true, typeof(WifiBand), WifiBand.TwoPointFourGHz, CultureInfo.InvariantCulture);
        result.Should().Be(WifiBand.TwoPointFourGHz);
    }

    [Fact]
    public void ConvertBack_WhenValueIsFalse_ReturnsDoNothing()
    {
        var result = _sut.ConvertBack(false, typeof(WifiBand), WifiBand.TwoPointFourGHz, CultureInfo.InvariantCulture);
        result.Should().Be(Avalonia.Data.BindingOperations.DoNothing);
    }

    [Fact]
    public void ConvertBack_WhenValueIsNull_ReturnsDoNothing()
    {
        var result = _sut.ConvertBack(null, typeof(WifiBand), WifiBand.TwoPointFourGHz, CultureInfo.InvariantCulture);
        result.Should().Be(Avalonia.Data.BindingOperations.DoNothing);
    }

    [Fact]
    public void Convert_WorksWithDifferentEnumTypes()
    {
        var result = _sut.Convert(DrivingMode.Racing, typeof(bool), DrivingMode.Racing, CultureInfo.InvariantCulture);
        result.Should().Be(true);
    }

    [Fact]
    public void Convert_WorksWithDifferentEnumTypes_ReturnsFalseForMismatch()
    {
        var result = _sut.Convert(DrivingMode.Standard, typeof(bool), DrivingMode.Racing, CultureInfo.InvariantCulture);
        result.Should().Be(false);
    }
}
