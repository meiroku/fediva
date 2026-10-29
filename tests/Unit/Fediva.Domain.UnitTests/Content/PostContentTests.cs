using Fediva.Domain.Common.Errors;
using Fediva.Domain.Content;

namespace Fediva.Domain.UnitTests.Content;

public class PostContentTests
{
    [Test]
    public async Task Create_WithValidText_Succeeds()
    {
        var result = PostContent.Create("Hello, world!");

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value.Text).IsEqualTo("Hello, world!");
    }

    [Test]
    public async Task Create_WithNullText_CreatesEmptyText()
    {
        var result = PostContent.Create(null);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value.Text).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task Create_WithEmptyText_Succeeds()
    {
        var result = PostContent.Create(string.Empty);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value.Text).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task Create_TrimsWhitespace()
    {
        var result = PostContent.Create("  trimmed  ");

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value.Text).IsEqualTo("trimmed");
    }

    [Test]
    public async Task Create_WithTextAtMaxLength_Succeeds()
    {
        var text = new string('a', PostContent.MaxLength);
        var result = PostContent.Create(text);

        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task Create_WithTextExceedingMaxLength_ReturnsFailure()
    {
        var text = new string('a', PostContent.MaxLength + 1);
        var result = PostContent.Create(text);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Error).IsEqualTo(ContentErrors.PostTextTooLong);
    }
}
