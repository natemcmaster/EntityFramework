// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal.Events;

namespace Microsoft.EntityFrameworkCore
{
    public static class RelationalEventBuilderExtensions
    {
        public static IEventHandler<RelationalConnectionOpened> OnConnectionOpened(
            this EventHandlerCollectionBuilder builder,
            Action<RelationalConnectionOpened> connection)
        {
            var handler = new DelegateDbContextEventHandler<RelationalConnectionOpened>(connection);
            builder.Handlers.Add(handler);
            return handler;
        }
    }
}
