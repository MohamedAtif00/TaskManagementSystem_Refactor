using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using TaskManagementSystem.TestCommon.MediatR;
using Xunit;

namespace TaskManagementSystem.Api.IntegrationTests;

public sealed class MediatrPipelineIntegrationTests : IClassFixture<MediatrPipelineWebApplicationFactory>
{
    private readonly MediatrPipelineWebApplicationFactory _factory;

    public MediatrPipelineIntegrationTests(MediatrPipelineWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Send_WhenCommandIsValid_ReturnsHandlerResult()
    {
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new ValidatedCommand("Integration"));

        result.Should().Be("Hello, Integration");
    }

    [Fact]
    public async Task Send_WhenCommandIsInvalid_ThrowsValidationException()
    {
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var act = () => mediator.Send(new ValidatedCommand(string.Empty));

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Name is required.*");
    }

    [Fact]
    public async Task Send_WhenPingCommand_ReturnsPong()
    {
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new PingCommand());

        result.Should().Be("pong");
    }
}

public sealed class MediatrPipelineWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.AddMediatR(configuration =>
                configuration.RegisterServicesFromAssembly(typeof(ValidatedCommand).Assembly));
            services.AddValidatorsFromAssembly(typeof(ValidatedCommand).Assembly);
        });
    }
}
