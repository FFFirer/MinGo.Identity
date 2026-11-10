// Copyright (c) SimpleIdServer. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using OpenTelemetry.Exporter;

namespace MinGo.IdServer.Admin;

public class IdentityServerAdminConfiguration
{
    // Keep IdserverBaseUrl public: it controls browser redirects and issuer validation.
    public string? BackchannelBaseUrl { get; set; }

    public int BackchannelTimeoutSeconds { get; set; } = 15;

    public bool ForceHttps
    {
        get; set;
    }

    public bool IsForwardedEnabled
    {
        get; set;
    }

    public bool IsRealmEnabled
    {
        get; set;
    }

    public string DataProtectionPath
    {
        get; set;
    } = null!;

    public string ScimBaseUrl
    {
        get; set;
    } = null!;
    
    public string IdserverBaseUrl
    {
        get; set;
    } = null!;

    public string ClientId
    {
        get; set;
    } = null!;

    public string ClientSecret
    {
        get; set;
    } = null!;

    public List<string> Scopes
    {
        get; set;
    } = null!;

    public bool IgnoreCertificateError
    {
        get; set;
    }

    public OpenTelemetryOptions OpenTelemetryOptions
    {
        get; set;
    } = null!;
}

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
