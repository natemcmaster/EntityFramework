// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal.Events;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities.FakeProvider
{
    public class FakeRelationalConnection : RelationalConnection
    {
        private readonly List<FakeDbConnection> _dbConnections = new List<FakeDbConnection>();

        public FakeRelationalConnection(IDbContextOptions options)
            : base(options, new Logger<FakeRelationalConnection>(new LoggerFactory()), new LifecycleManager(options, Enumerable.Empty<EventHandlerCollection>()))
        {
        }

        public IReadOnlyList<FakeDbConnection> DbConnections => _dbConnections;

        protected override DbConnection CreateDbConnection()
        {
            var connection = new FakeDbConnection(ConnectionString);

            _dbConnections.Add(connection);

            return connection;
        }
    }
}
