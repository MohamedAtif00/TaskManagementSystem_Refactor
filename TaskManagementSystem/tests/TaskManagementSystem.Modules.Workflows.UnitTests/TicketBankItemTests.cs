using FluentAssertions;
using TaskManagementSystem.Modules.Workflows.Domain;
using Xunit;

namespace TaskManagementSystem.Modules.Workflows.UnitTests;

public sealed class TicketBankItemTests
{
    [Fact]
    public void Create_WhenNameEmpty_Fails()
    {
        var result = TicketBankItem.Create(string.Empty, 10, TicketBankType.Creation, false, 1);
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("ticket_bank_invalid_name");
    }

    [Fact]
    public void Create_WhenValid_Succeeds()
    {
        var result = TicketBankItem.Create("Design", 60, TicketBankType.Creation, true, 3);
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Design");
        result.Value.Active.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_SetsActiveFalse()
    {
        var item = TicketBankItem.Create("Design", 60, TicketBankType.Creation, false, 3).Value;
        item.Deactivate();
        item.Active.Should().BeFalse();
    }
}
