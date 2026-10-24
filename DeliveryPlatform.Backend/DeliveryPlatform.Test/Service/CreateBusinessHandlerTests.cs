using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Features.Business.Create;
using DeliveryPlatform.Application.src.Features.Business.GetMy;
using DeliveryPlatform.Domain.src.Entity;
using Moq;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class CreateBusinessHandlerTests
{
    private readonly Mock<IBusinessRepository> _businessRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateBusinessHandler _handler;

    public CreateBusinessHandlerTests()
    {
        _businessRepositoryMock = new Mock<IBusinessRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateBusinessHandler(_businessRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateBusiness_WhenValidRequest()
    {
        var userId = Guid.NewGuid();
        var command = new CreateBusinessCommand("TestShop", "Desc", BusinessType.Restaurant, userId);

        _businessRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Business>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result);
        _businessRepositoryMock.Verify(r => r.AddAsync(It.Is<Business>(b => b.Name == "TestShop" && b.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenNameIsEmpty()
    {
        var command = new CreateBusinessCommand(" ", "Desc", BusinessType.Restaurant, Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
