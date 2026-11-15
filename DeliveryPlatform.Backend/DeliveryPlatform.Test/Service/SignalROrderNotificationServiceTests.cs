using DeliveryPlatform.API.Hubs;
using DeliveryPlatform.API.Services.Notifications;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class SignalROrderNotificationServiceTests
{
    private readonly Mock<IHubContext<OrderHub>> _hubContextMock;
    private readonly Mock<IHubClients> _clientsMock;
    private readonly Mock<IClientProxy> _clientProxyMock;
    private readonly SignalROrderNotificationService _service;

    public SignalROrderNotificationServiceTests()
    {
        _hubContextMock = new Mock<IHubContext<OrderHub>>();
        _clientsMock = new Mock<IHubClients>();
        _clientProxyMock = new Mock<IClientProxy>();

        _hubContextMock.SetupGet(h => h.Clients).Returns(_clientsMock.Object);
        _clientsMock
            .Setup(c => c.Group(It.IsAny<string>()))
            .Returns(_clientProxyMock.Object);
        _clientProxyMock
            .Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _service = new SignalROrderNotificationService(_hubContextMock.Object);
    }

    [Fact]
    public async Task NotifyOrderStatusChangedAsync_ShouldSendToUserGroup()
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        await _service.NotifyOrderStatusChangedAsync(userId, orderId, "Delivered");

        _clientsMock.Verify(c => c.Group($"user-{userId}"), Times.Once);
        _clientProxyMock.Verify(
            p => p.SendCoreAsync(
                "OrderStatusChanged",
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task NotifyOrderStatusChangedAsync_ShouldSendOrderIdAndStatusPayload()
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        object?[]? capturedArgs = null;

        _clientProxyMock
            .Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Callback<string, object?[], CancellationToken>((_, args, _) => capturedArgs = args)
            .Returns(Task.CompletedTask);

        await _service.NotifyOrderStatusChangedAsync(userId, orderId, "OutForDelivery");

        var payload = Assert.Single(capturedArgs!);
        Assert.NotNull(payload);
        var payloadType = payload!.GetType();
        Assert.Equal(orderId, payloadType.GetProperty("orderId")!.GetValue(payload));
        Assert.Equal("OutForDelivery", payloadType.GetProperty("newStatus")!.GetValue(payload));
        var changedAt = Assert.IsType<DateTime>(payloadType.GetProperty("ChangedAt")!.GetValue(payload));
        Assert.True(changedAt > DateTime.UtcNow.AddMinutes(-1));
        Assert.True(changedAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task NotifyOrderStatusChangedAsync_ShouldPropagateFailure()
    {
        _clientProxyMock
            .Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("send failed"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.NotifyOrderStatusChangedAsync(Guid.NewGuid(), Guid.NewGuid(), "Confirmed"));
    }
}
