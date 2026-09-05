namespace TaskManagementSystem.Api.Configuration;

public sealed class OpenTelemetryOptions
{
    public const string SectionName = "OpenTelemetry";

    public string ServiceName { get; set; } = BuildingBlocks.Infrastructure.Observability.Telemetry.ServiceName;

    /// <summary>
    /// OTLP collector endpoint (e.g. http://localhost:4317). Empty disables export.
    /// Falls back to OTEL_EXPORTER_OTLP_ENDPOINT when unset.
    /// </summary>
    public string? OtlpEndpoint { get; set; }
}
