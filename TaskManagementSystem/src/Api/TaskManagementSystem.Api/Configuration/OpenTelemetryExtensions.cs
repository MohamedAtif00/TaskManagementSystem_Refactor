using System.Reflection;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Observability;

namespace TaskManagementSystem.Api.Configuration;

public static class OpenTelemetryExtensions
{
    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        var options = builder.Configuration
            .GetSection(OpenTelemetryOptions.SectionName)
            .Get<OpenTelemetryOptions>() ?? new OpenTelemetryOptions();

        var serviceName = string.IsNullOrWhiteSpace(options.ServiceName)
            ? Telemetry.ServiceName
            : options.ServiceName;

        var serviceVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";

        var otlpEndpoint = FirstNonEmpty(
            options.OtlpEndpoint,
            Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT"));

        var exportOtlp = Uri.TryCreate(otlpEndpoint, UriKind.Absolute, out var otlpUri);

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
            logging.ParseStateValues = true;
            logging.SetResourceBuilder(
                ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion: serviceVersion));

            if (exportOtlp)
            {
                logging.AddOtlpExporter(exporter => exporter.Endpoint = otlpUri!);
            }
        });

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName, serviceVersion: serviceVersion))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource($"{Telemetry.ActivitySourcePrefix}.*");

                foreach (var sourceName in Telemetry.ActivitySourceNames)
                {
                    tracing.AddSource(sourceName);
                }

                if (exportOtlp)
                {
                    tracing.AddOtlpExporter(exporter => exporter.Endpoint = otlpUri!);
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                foreach (var meterName in Telemetry.MeterNames)
                {
                    metrics.AddMeter(meterName);
                }

                if (exportOtlp)
                {
                    metrics.AddOtlpExporter(exporter => exporter.Endpoint = otlpUri!);
                }
            });

        return builder;
    }

    private static string? FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
}
