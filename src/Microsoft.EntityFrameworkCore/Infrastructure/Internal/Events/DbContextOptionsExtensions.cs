// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal.Events;
using Microsoft.Extensions.DependencyInjection;
#pragma warning disable 1591

namespace Microsoft.EntityFrameworkCore
{
    public static class DbContextOptionsExtensions
    {
        public static EventHandlerCollectionBuilder ConfigureEvents(this DbContextOptionsBuilder optionsBuilder)
        {
            var extension = optionsBuilder.Options.FindExtension<EventExtension>();
            if (extension == null)
            {
                extension = new EventExtension();
                ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);
            }

            return new EventHandlerCollectionBuilder(extension.Handlers);
        }

        public static DbContextOptionsBuilder ConfigureEvents(this DbContextOptionsBuilder optionsBuilder, Action<EventHandlerCollectionBuilder> configure)
        {
            configure(ConfigureEvents(optionsBuilder));
            return optionsBuilder;
        }
    }

    public class EventExtension : IDbContextOptionsExtension
    {
        public EventHandlerCollection Handlers { get; set; } = new EventHandlerCollection();

        public void ApplyServices([NotNull] IServiceCollection services)
        {
        }
    }
}
