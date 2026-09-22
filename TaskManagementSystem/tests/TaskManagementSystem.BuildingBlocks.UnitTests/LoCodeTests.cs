using FluentAssertions;
using TaskManagementSystem.BuildingBlocks.Domain;
using Xunit;

namespace TaskManagementSystem.BuildingBlocks.UnitTests;

public sealed class LoCodeTests
{
    [Theory]
    [InlineData("Mth_5R_1A_01_04_02", "mth", 5, 1, 'A', 1, 4, 2)]
    [InlineData("Ara_5R_1A_01_01_04", "ara", 5, 1, 'A', 1, 1, 4)]
    [InlineData("Sci_5R_1A_04_03_05", "sci", 5, 1, 'A', 4, 3, 5)]
    [InlineData("Eng_5R_1E_07_04_04", "eng", 5, 1, 'E', 7, 4, 4)]
    [InlineData("Soc_5R_1A_02_03_02", "soc", 5, 1, 'A', 2, 3, 2)]
    [InlineData("Mul_2R_1E_01_03_01", "mul", 2, 1, 'E', 1, 3, 1)]
        [InlineData("Rel_3R_1A_05_02_01", "rel", 3, 1, 'A', 5, 2, 1)]
        [InlineData("Ict_5R_1A_01_02_01", "ict", 5, 1, 'A', 1, 2, 1)]
        [InlineData("Tsk_1R_1A_01_01_01", "tsk", 1, 1, 'A', 1, 1, 1)]
    [InlineData("2026_eng_4r_1e_02_02_03", "eng", 4, 1, 'E', 2, 2, 3)]
    [InlineData("2026_ara_2r_1a_02_05_03", "ara", 2, 1, 'A', 2, 5, 3)]
    [InlineData("QR_mth_1r_1a_02_02_06", "mth", 1, 1, 'A', 2, 2, 6)]
    [InlineData("QR_2025_ara_3r_1a_01_01_01", "ara", 3, 1, 'A', 1, 1, 1)]
    [InlineData("Soc_4R_1A_01_04_03_p2", "soc", 4, 1, 'A', 1, 4, 3)]
    public void TryParse_WhenCodeMatchesGrammar_ReturnsParts(
        string raw,
        string subject,
        int grade,
        int term,
        char track,
        int unit,
        int lesson,
        int loIndex)
    {
        var parsed = LoCodeParser.TryParse(raw, out var code);

        parsed.Should().BeTrue();
        code.Should().NotBeNull();
        code!.SubjectCode.Should().Be(subject);
        code.Grade.Should().Be(grade);
        code.TermNumber.Should().Be(term);
        code.Track.Should().Be(track);
        code.Unit.Should().Be(unit);
        code.Lesson.Should().Be(lesson);
        code.LoIndex.Should().Be(loIndex);
    }

    [Fact]
    public void TryParse_WhenYearAndQrPrefixPresent_CapturesOptionalSegments()
    {
        LoCodeParser.TryParse("QR_2025_ara_3r_1a_01_01_01", out var code).Should().BeTrue();

        code!.Prefix.Should().Be("QR");
        code.Year.Should().Be(2025);
        code.Suffix.Should().BeNull();
    }

    [Fact]
    public void TryParse_WhenPartSuffixPresent_CapturesSuffix()
    {
        LoCodeParser.TryParse("Soc_4R_1A_01_04_03_p2", out var code).Should().BeTrue();

        code!.Suffix.Should().Be("p2");
        code.Prefix.Should().BeNull();
        code.Year.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Solve linear equations")]
    [InlineData("Mth_5R")]
    [InlineData("Mth_5R_1A_01_04")]
    [InlineData("Math_5R_1A_01_04_02")]
    public void TryParse_WhenInputIsNotACode_ReturnsFalse(string? raw)
    {
        LoCodeParser.TryParse(raw, out var code).Should().BeFalse();
        code.Should().BeNull();
    }

    [Fact]
    public void Format_WhenEnglish_UsesDefaultLabels()
    {
        LoCodeFormatter.Format("Mth_5R_1A_01_04_02")
            .Should().Be("Math · Grade 5 · Term 1 (Arabic) · Unit 1 · Lesson 4 · LO 2");
        LoCodeFormatter.Format("Eng_5R_1E_07_04_04", "en")
            .Should().Be("English · Grade 5 · Term 1 (English) · Unit 7 · Lesson 4 · LO 4");
        LoCodeFormatter.Format("2026_eng_4r_1e_02_02_03")
            .Should().Be("2026 · English · Grade 4 · Term 1 (English) · Unit 2 · Lesson 2 · LO 3");
        LoCodeFormatter.Format("QR_mth_1r_1a_02_02_06")
            .Should().Be("QR · Math · Grade 1 · Term 1 (Arabic) · Unit 2 · Lesson 2 · LO 6");
        LoCodeFormatter.Format("QR_2025_ara_3r_1a_01_01_01")
            .Should().Be("QR · 2025 · Arabic · Grade 3 · Term 1 (Arabic) · Unit 1 · Lesson 1 · LO 1");
        LoCodeFormatter.Format("Soc_4R_1A_01_04_03_p2")
            .Should().Be("Social Studies · Grade 4 · Term 1 (Arabic) · Unit 1 · Lesson 4 · LO 3 · Part 2");
    }

    [Fact]
    public void Format_WhenArabic_UsesArabicLabels()
    {
        LoCodeFormatter.Format("Mth_5R_1A_01_04_02", "ar")
            .Should().Be("الرياضيات · الصف 5 · الفصل الدراسي الأول (عربي) · الوحدة 1 · الدرس 4 · الهدف 2");
        LoCodeFormatter.Format("Eng_5R_1E_07_04_04", LoCodeLanguage.Ar)
            .Should().Be("اللغة الإنجليزية · الصف 5 · الفصل الدراسي الأول (إنجليزي) · الوحدة 7 · الدرس 4 · الهدف 4");
        LoCodeFormatter.Format("2026_ara_2r_1a_02_05_03", "ar")
            .Should().Be("2026 · اللغة العربية · الصف 2 · الفصل الدراسي الأول (عربي) · الوحدة 2 · الدرس 5 · الهدف 3");
        LoCodeFormatter.Format("Soc_4R_1A_01_04_03_p2", "ar")
            .Should().Be("الدراسات الاجتماعية · الصف 4 · الفصل الدراسي الأول (عربي) · الوحدة 1 · الدرس 4 · الهدف 3 · الجزء 2");
    }

    [Theory]
    [InlineData("Solve linear equations")]
    [InlineData("not-a-code")]
    public void Format_WhenUnparseable_ReturnsOriginalName(string raw)
    {
        LoCodeFormatter.Format(raw).Should().Be(raw);
        LoCodeFormatter.Format(raw, "ar").Should().Be(raw);
    }

    [Fact]
    public void Format_WhenRawIsNull_ReturnsEmpty()
    {
        LoCodeFormatter.Format((string?)null).Should().BeEmpty();
    }

    [Fact]
    public void LoCode_WhenPartsMatch_AreEqual()
    {
        LoCodeParser.TryParse("Mth_5R_1A_01_04_02", out var left);
        LoCodeParser.TryParse("mth_5r_1a_01_04_02", out var right);

        left.Should().Be(right);
    }
}
