using SongTheSame.Domain.Songs;

namespace SongTheSame.Domain.Tests.Songs;

public class SearchTextTests
{
    [Theory]
    [InlineData("Moja Štikla", "moja stikla")]
    [InlineData("Đurđevdan — Goran Bregović", "durdevdan goran bregovic")]
    [InlineData("AC/DC - Back In Black (Remastered)", "ac dc back in black remastered")]
    [InlineData("  Beyoncé   ", "beyonce")]
    public void Normalize_strips_accents_case_and_punctuation(string input, string expected)
    {
        SearchText.Normalize(input).ShouldBe(expected);
    }

    [Fact]
    public void Tokenize_splits_into_normalised_words()
    {
        SearchText.Tokenize("Čaj Šuškov").ShouldBe(["caj", "suskov"]);
    }
}
