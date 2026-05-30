// Copyright (c) SimpleIdServer. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
namespace MinGo.IdServer.Configurations;

public class MessageBrokerOptions
{
    public TransportTypes Transport { get; set; }
    public string ConnectionString { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Database { get; set; } = null!;
}

public enum TransportTypes
{
    INMEMORY = 0,
    SQLSERVER = 1
}