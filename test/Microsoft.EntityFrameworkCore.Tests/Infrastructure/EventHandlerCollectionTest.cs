// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal.Events;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.Infrastructure
{
    public class EventHandlerCollectionTest
    {
        [Fact]
        public void CachesHandlersOnType()
        {
            var collection = new EventHandlerCollection();
            var called = false;
            collection.Add(new DelegateEventHandler<int>(_ => called = true));
            collection.Add(
                new DelegateEventHandler<string>(msg => { throw new InvalidOperationException(msg); }));

            collection.Raise(default(int));

            Assert.True(called);
            Assert.Equal("It threw",
                Assert.Throws<InvalidOperationException>(() => collection.Raise("It threw")).Message);
        }

        [Fact]
        public void Remove()
        {
            var collection = new EventHandlerCollection();
            var expected = 0;
            var count = 0;
            var handler = new DelegateEventHandler<int>(c => count += c);

            // noop
            collection.Remove(handler);

            // add again
            collection.Add(handler);
            expected += 1;
            collection.Raise(1);
            Assert.Equal(expected, count);

            // add multiple
            collection.Add(handler);
            expected += 2;
            collection.Raise(1);
            Assert.Equal(expected, count);

            // remove once
            collection.Remove(handler);
            expected += 1;
            collection.Raise(1);
            Assert.Equal(expected, count);
        }
    }
}
