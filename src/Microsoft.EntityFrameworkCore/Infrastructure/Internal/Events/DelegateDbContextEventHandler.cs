// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
#pragma warning disable 1591

using System;

namespace Microsoft.EntityFrameworkCore.Infrastructure.Internal.Events
{
    public class DelegateDbContextEventHandler<TEvent> : DelegateEventHandler<TEvent>
        where TEvent : DbContextEvent
    {
        public DelegateDbContextEventHandler(Action<TEvent> @delegate)
            : base(@delegate)
        {
        }

        public override void Handle(TEvent @event)
        {
            @event.Handled = true;
            base.Handle(@event);
        }
    }
}
