using Mkx.Templates.Shared.Utilities;

namespace Mkx.Templates.Tests;

public sealed class OtpCodeTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("۰۰۱۲۳۴", "001234")]
    [InlineData("٠١٢٣٤٥", "012345")]
    [InlineData("12 ۳-٤۵۶789", "123456")]
    [InlineData("text", "")]
    public void NormalizationPreservesLeadingZerosAndSupportsPastedDigits(string? input, string expected)
        => Assert.Equal(expected, OtpCode.Normalize(input));
}
