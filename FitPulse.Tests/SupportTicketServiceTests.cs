namespace FitPulse.Tests;

public class SupportTicketServiceTests
{
    [Fact]
    public async Task CreateTicketAsync_ZetStatusOpenEnRoeptRepositoryAan()
    {
        var repositoryMock = new Mock<ISupportTicketRepository>();
        var service = new SupportTicketService(repositoryMock.Object);

        var ticket = await service.CreateTicketAsync(memberId: 1, "Probleem", "Omschrijving", TicketPriority.High);

        ticket.Status.Should().Be(TicketStatus.Open);
        ticket.MemberId.Should().Be(1);
        repositoryMock.Verify(r => r.AddAsync(ticket), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_BestaandTicket_WijzigtStatus()
    {
        var existingTicket = new SupportTicket { Id = 1, MemberId = 1, Subject = "Test", Description = "Test", Status = TicketStatus.Open };
        var repositoryMock = new Mock<ISupportTicketRepository>();
        repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingTicket);
        var service = new SupportTicketService(repositoryMock.Object);

        var result = await service.UpdateStatusAsync(1, TicketStatus.InProgress);

        result.Status.Should().Be(TicketStatus.InProgress);
        repositoryMock.Verify(r => r.UpdateAsync(existingTicket), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_TicketBestaatNiet_GooitKeyNotFoundException()
    {
        var repositoryMock = new Mock<ISupportTicketRepository>();
        repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((SupportTicket?)null);
        var service = new SupportTicketService(repositoryMock.Object);

        Func<Task> act = () => service.UpdateStatusAsync(999, TicketStatus.Resolved);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}