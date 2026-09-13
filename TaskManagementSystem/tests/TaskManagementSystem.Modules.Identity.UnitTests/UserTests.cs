using FluentAssertions;
using TaskManagementSystem.Modules.Identity.Domain;
using Xunit;

namespace TaskManagementSystem.Modules.Identity.UnitTests;

public sealed class UserTests
{
    [Fact]
    public void Create_WhenNonOwnerWithoutTeam_ReturnsError()
    {
        var result = User.Create(
            "ABC123",
            "Jane Doe",
            "HR001",
            null,
            null,
            null,
            (int)UserRole.Member,
            AccountType.Internal,
            null,
            null);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("team_required");
    }

    [Fact]
    public void Create_WhenOwnerWithTeam_ReturnsError()
    {
        var result = User.Create(
            "ABC123",
            "Owner User",
            "HR001",
            null,
            null,
            null,
            (int)UserRole.Owner,
            AccountType.Internal,
            1,
            null);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("team_not_allowed");
    }

    [Fact]
    public void Create_WhenMemberWithValidData_CreatesUser()
    {
        var result = User.Create(
            "ABC123",
            "Jane Doe",
            "HR001",
            "jane@example.com",
            "0100",
            "Engineer",
            (int)UserRole.Member,
            AccountType.Internal,
            1,
            2);

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("jane@example.com");
        result.Value.TeamId.Should().Be(1);
        result.Value.TeamleaderId.Should().Be(2);
        result.Value.AnnualLeaveMax.Should().Be(30);
    }

    [Fact]
    public void Archive_SetsArchivedFlag()
    {
        var user = User.Create(
            "ABC123",
            "Jane Doe",
            "HR001",
            null,
            null,
            null,
            (int)UserRole.Member,
            AccountType.Internal,
            1,
            null).Value;

        user.Archive();

        user.Archived.Should().BeTrue();
        user.CanAuthenticate().IsSuccess.Should().BeFalse();
    }
}
