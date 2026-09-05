using System.Diagnostics;
using System.Diagnostics.Metrics;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Observability;

namespace TaskManagementSystem.BuildingBlocks.Application.Behaviors;

public sealed class TracingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly Counter<long> RequestCounter =
        Telemetry.Mediator.Meter.CreateCounter<long>("tms.mediatr.requests");

    private static readonly Histogram<double> DurationHistogram =
        Telemetry.Mediator.Meter.CreateHistogram<double>("tms.mediatr.duration_ms");

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest);
        var requestName = MediatRRequestMetadata.GetRequestName(requestType);
        var kind = MediatRRequestMetadata.GetKind(requestType);
        var module = MediatRRequestMetadata.GetModuleName(requestType);

        using var activity = Telemetry.Mediator.ActivitySource.StartActivity(requestName);
        activity?.SetTag("mediatr.request_name", requestName);
        activity?.SetTag("mediatr.kind", kind);
        activity?.SetTag("mediatr.module", module);

        var stopwatch = Stopwatch.StartNew();
        var success = false;

        try
        {
            var response = await next(cancellationToken);
            success = true;
            return response;//
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
            activity?.AddException(exception);
            throw;
        }
        finally
        {
            stopwatch.Stop();

            var tags = new TagList
            {
                { "mediatr.kind", kind },
                { "mediatr.module", module },
                { "success", success }
            };

            RequestCounter.Add(1, tags);
            DurationHistogram.Record(stopwatch.Elapsed.TotalMilliseconds, tags);
        }
    }
}
