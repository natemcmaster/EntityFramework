// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Infrastructure.Internal.Events
{
    public class RelationalConnectionOpened : DbContextEvent
    {
        public RelationalConnectionOpened(IRelationalConnection connection, bool async)
        {
            Connection = connection;
            WasAsync = async;
        }

        public IRelationalConnection Connection { get; }
        public bool WasAsync { get; }
    }
}
