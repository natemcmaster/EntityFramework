// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Design.Internal.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public class DesignClientBuilder
    {
        private readonly DesignClientOptions _options = new DesignClientOptions();

        public DesignClientBuilder WithDesignServer(string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri))
            {
                throw new ArgumentException("Invalid url", nameof(url));
            }
            _options.Server = uri;
            return this;
        }

        public DesignClient Build()
        {
            var services = new ServiceCollection()
                .AddSingleton(_options)
                .AddSingleton<DesignClient>()
                .AddSingleton<IOperationExecutor, OperationExecutor>()
                .AddSingleton<ICommunicationService, HttpCommunicationService>()
                .BuildServiceProvider();

            var client = services.GetRequiredService<DesignClient>();

            return client;
        }
    }
}
