// Copyright (c) SimpleIdServer. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using MinGo.Identity.Configuration;

namespace MinGo.IdServer.Configurations;

public class OpeniddictMigrationOptions
{
    public StorageTypes Transport
    {
        get; set;
    }

    public string ConnectionString
    {
        get; set;
    }
}
