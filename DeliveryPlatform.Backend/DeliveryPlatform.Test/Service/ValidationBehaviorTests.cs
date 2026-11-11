using DeliveryPlatform.Application.src.Common.Behaviors;
using FluentValidation;
using MediatR;
using Xunit;

namespace DeliveryPlatform.Test.Service;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_ShouldCallNext_WhenThereAreNoValidators()
    {
        var behavior = new ValidationBehavior<TestRequest, string>(Array.Empty<IValidator<TestRequest>>());

        var result = await behavior.Handle(
            new TestRequest("value"),
            _ => Task.FromResult("ok"),
            CancellationToken.None);

        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task Handle_ShouldCallNext_WhenValidationPasses()
    {
        var validator = new InlineValidator<TestRequest>();
        validator.RuleFor(x => x.Value).NotEmpty();
        var behavior = new ValidationBehavior<TestRequest, string>(new[] { validator });

        var result = await behavior.Handle(
            new TestRequest("value"),
            _ => Task.FromResult("ok"),
            CancellationToken.None);

        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenValidationFails()
    {
        var validator = new InlineValidator<TestRequest>();
        validator.RuleFor(x => x.Value).NotEmpty();
        var behavior = new ValidationBehavior<TestRequest, string>(new[] { validator });
        var nextCalled = false;

        await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(
                new TestRequest(string.Empty),
                _ =>
                {
                    nextCalled = true;
                    return Task.FromResult("ok");
                },
                CancellationToken.None));

        Assert.False(nextCalled);
    }

    private sealed record TestRequest(string Value) : IRequest<string>;
}
