// Copyright (c) SimpleIdServer. AllClients rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using OpenTelemetry.Exporter;

namespace MinGo.IdServer.Configurations;

public class OpenTelemetryOptions
{
    public bool EnableOtpExported
    {
        get; set;
    }

    public bool EnableConsoleExporter 
    { 
        get; set;
    } = true;

    public bool EnableEfCoreTracing
    {
        get; set;
    } = true;

    public string MetricsEndpoint
    { 
        get; set;
    } = null!;

    public string TracesEndpoint
    {
        get; set;
    } = null!;

    public string Headers
    {
        get; set;
    } = null!;

    public OtlpExportProtocol Protocol
    {
        get; set;
    } = OtlpExportProtocol.HttpProtobuf;
}
