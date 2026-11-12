using SongTheSame.Domain.Common;

namespace SongTheSame.Domain.Tests.Common;

public class ShareCodeTests
{
    [Fact]
    public void Generated_codes_have_the_expected_length_and_avoid_confusing_characters()
    {
        for (var i = 0; i < 200; i++)
        {
            var code = ShareCode.Generate();

            code.Length.ShouldBe(ShareCode.Length);
            code.ShouldAllBe(c => "01OIL".IndexOf(c) < 0);
        }
    }

    [Fact]
    public void Normalize_is_forgiving_about_case_spaces_and_dashes()
    {
        ShareCode.Normalize(" abcd-efgh ").ShouldBe("ABCDEFGH");
    }
}
