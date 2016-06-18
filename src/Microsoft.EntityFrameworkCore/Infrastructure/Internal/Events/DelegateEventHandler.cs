// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
#pragma warning disable 1591

using System;

namespace Microsoft.EntityFrameworkCore.Infrastructure.Internal.Events
{
    public class DelegateEventHandler<TEvent> : IEventHandler<TEvent>
    {
        public virtual Action<TEvent> Delegate { get; }

        public DelegateEventHandler(Action<TEvent> @delegate)
        {
            Delegate = @delegate;
        }

        public virtual void Handle(TEvent @event) => Delegate.Invoke(@event);
    }
}
