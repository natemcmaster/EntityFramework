// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
#pragma warning disable 1591

namespace Microsoft.EntityFrameworkCore.Infrastructure.Internal.Events
{
    public static class EventHandlerCollectionExtensions
    {
        public static void Raise<TEvent>(this EventHandlerCollection collection, TEvent @event)
        {
            foreach (var handlers in collection.Find<TEvent>())
            {
                handlers.Handle(@event);
            }
        }
    }
}
