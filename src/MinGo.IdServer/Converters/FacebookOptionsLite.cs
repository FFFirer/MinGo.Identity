// Copyright (c) SimpleIdServer. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Microsoft.AspNetCore.Authentication.Facebook;
using SimpleIdServer.Configuration;
using SimpleIdServer.IdServer.UI.AuthProviders;

namespace MinGo.IdServer.Converters
{
    public class FacebookOptionsLite : IDynamicAuthenticationOptions<FacebookOptions>
    {
        [ConfigurationRecord("AppId", "Application identifier", 0, IsRequired = true)]
        public string AppId { get; set; } = null!;
        [ConfigurationRecord("AppSecret", "Application secret", 1, null, CustomConfigurationRecordType.PASSWORD, IsRequired = true)]
        public string AppSecret { get; set; } = null!;

        public FacebookOptions Convert() => new FacebookOptions
        {
            AppId = AppId,
            AppSecret = AppSecret
        };
    }
}