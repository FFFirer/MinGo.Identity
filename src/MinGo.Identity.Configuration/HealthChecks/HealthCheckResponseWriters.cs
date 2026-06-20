using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MinGo.Identity.Configuration;

/// <summary>
/// Custom response writers for health check endpoints.
/// Produces structured JSON with component-level status, description, and duration.
/// </summary>
internal static class HealthCheckResponseWriters
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static Task WriteJson(HttpContext context, HealthReport report)
    {
        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.ToString("g"),
            entries = report.Entries.ToDictionary(
                e => e.Key,
                e => new
                {
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.ToString("g"),
                    exception = e.Value.Exception?.Message
                })
        };

        context.Response.ContentType = "application/json; charset=utf-8";
        return context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
