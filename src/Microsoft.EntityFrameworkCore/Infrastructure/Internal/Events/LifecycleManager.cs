// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
#pragma warning disable 1591

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.EntityFrameworkCore.Infrastructure.Internal.Events
{
    public class LifecycleManager
    {
        private readonly IList<EventHandlerCollection> _groups;

        public LifecycleManager(IDbContextOptions options, IEnumerable<EventHandlerCollection> collections)
        {
            _groups = collections
                .Concat(new[] { options.FindExtension<EventExtension>()?.Handlers })
                .Where(c => c != null)
                .ToList();
        }

        public virtual void Trigger<TEvent>(TEvent arg)
        {
            foreach (var collection in _groups)
            {
                collection.Raise(arg);
            }
        }
    }
}
