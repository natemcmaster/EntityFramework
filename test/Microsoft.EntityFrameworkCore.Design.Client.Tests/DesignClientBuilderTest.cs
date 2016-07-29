// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.EntityFrameworkCore.Design.Client.Tests
{
    public class DesignClientBuilderTest
    {
        [Fact]
        public void Build()
        {
            var client = new DesignClientBuilder().Build();

            Assert.NotNull(client);
        }
    }
}
