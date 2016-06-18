// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
#pragma warning disable 1591

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.EntityFrameworkCore.Infrastructure.Internal.Events
{
    public class EventHandlerCollection
    {
        private readonly Dictionary<Type, ICollection<IEventHandler>> _eventHandlers
            = new Dictionary<Type, ICollection<IEventHandler>>();

        public virtual void Add<TEvent>(IEventHandler<TEvent> handler)
        {
            ICollection<IEventHandler> all;
            if (!_eventHandlers.TryGetValue(typeof(TEvent), out all))
            {
                all = new List<IEventHandler>();
                _eventHandlers.Add(typeof(TEvent), all);
            }
            all.Add(handler);
        }

        public virtual void Remove<TEvent>(IEventHandler<TEvent> handler)
        {
            ICollection<IEventHandler> all;
            if (!_eventHandlers.TryGetValue(typeof(TEvent), out all))
            {
                return;
            }
            all.Remove(handler);
        }

        public virtual IEnumerable<IEventHandler<TEvent>> Find<TEvent>()
        {
            ICollection<IEventHandler> all;
            return !_eventHandlers.TryGetValue(typeof(TEvent), out all)
                ? Enumerable.Empty<IEventHandler<TEvent>>()
                : all.Cast<IEventHandler<TEvent>>();
        }
    }
}
