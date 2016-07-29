// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Design.Internal.Protocol;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Design.Client.Tests
{
    public class DesignClientTest
    {
        [Fact]
        public void ThrowsOnBadSettingsResponse()
        {
            var communicator = new MockCommunicationService();
            communicator.Enqueue<DesignServerSettings>(null);
            var client = new DesignClient(communicator, new MockExecutor())
                .Start()
                .Wait();

            Assert.Equal(1, communicator.Gets<DesignServerSettings>());
            Assert.IsType<InvalidOperationException>(client.Error);
            Assert.True(client.Finished);
        }

        [Fact]
        public void ExitsWhenQueueEmpties()
        {
            var communicator = new MockCommunicationService();
            communicator.Enqueue(new DesignServerSettings { StayAlive = false });
            communicator.Enqueue<ICollection<DesignOperation>>(new[] { new DesignOperation() });
            communicator.Enqueue<ICollection<DesignOperation>>(new DesignOperation[0]);
            communicator.Enqueue(new OperationResultSubmitted { ServerRequestedClientShutdown = false });

            var client = new DesignClient(communicator, new MockExecutor()) { RetryTimeout = 0 }.Start().Wait();

            Assert.Equal(0, communicator.QueueCount<ICollection<DesignOperation>>());
            Assert.Equal(2, communicator.Gets<ICollection<DesignOperation>>());
            Assert.Equal(1, communicator.Sends<OperationResult>());
            Assert.Null(client.Error);
            Assert.True(client.Finished);
        }

        [Fact]
        public void StayAliveWaitsForServerShutdownRequest()
        {
            var communicator = new MockCommunicationService();
            communicator.Enqueue(new DesignServerSettings { StayAlive = true });
            communicator.Enqueue<ICollection<DesignOperation>>(new DesignOperation[0]);
            communicator.Enqueue<ICollection<DesignOperation>>(new DesignOperation[0]);
            communicator.Enqueue<ICollection<DesignOperation>>(new DesignOperation[0]);
            communicator.Enqueue<ICollection<DesignOperation>>(new[] { new DesignOperation() });
            communicator.Enqueue(new OperationResultSubmitted { ServerRequestedClientShutdown = true });

            var client = new DesignClient(communicator, new MockExecutor()) { RetryTimeout = 0 }.Start().Wait();

            Assert.Equal(0, communicator.QueueCount<ICollection<DesignOperation>>());
            Assert.Equal(4, communicator.Gets<ICollection<DesignOperation>>());
            Assert.Equal(1, communicator.Sends<OperationResult>());
            Assert.Null(client.Error);
            Assert.True(client.Finished);
        }
    }

    public class MockExecutor : IOperationExecutor
    {
        public Task<OperationResult> ExecuteAsync(DesignOperation operation, CancellationToken cancellationToken = default(CancellationToken)) 
            => Task.FromResult<OperationResult>(null);
    }

    public class MockCommunicationService : ICommunicationService
    {
        private readonly Dictionary<Type, Queue<object>> _responses = new Dictionary<Type, Queue<object>>();
        private readonly Dictionary<Type, int> _gets = new Dictionary<Type, int>();
        private readonly Dictionary<Type, int> _sends = new Dictionary<Type, int>();

        public void Enqueue<T>(T obj)
        {
            Queue<object> queue;
            if (!_responses.TryGetValue(typeof(T), out queue))
            {
                queue = new Queue<object>();
                _responses.Add(typeof(T), queue);
            }
            queue.Enqueue(obj);
        }

        public int QueueCount<T>() => _responses[typeof(T)].Count;
        public int Gets<T>() => _gets[typeof(T)];
        public int Sends<T>() => _sends[typeof(T)];

        public Task<T> GetAsync<T>(CancellationToken cancellationToken = default(CancellationToken))
        {
            int count;
            if (!_gets.TryGetValue(typeof(T), out count))
            {
                count = 0;
            }
            _gets[typeof(T)] = count + 1;

            cancellationToken.ThrowIfCancellationRequested();
            var task = new TaskCompletionSource<T>();
            var queue = _responses[typeof(T)];
            if (!queue.Any())
                throw new InvalidOperationException("Queue empty for " + typeof(T).GetTypeInfo().FullName);
            task.TrySetResult((T)queue.Dequeue());

            return task.Task;
        }

        public Task SendAsync<T>(T instance, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            int count;
            if (!_sends.TryGetValue(typeof(T), out count))
            {
                count = 0;
            }
            _sends[typeof(T)] = count + 1;

            return Task.FromResult(default(T));
        }

        public Task<TResult> SendAsync<T, TResult>(T instance, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            SendAsync(instance);
            return GetAsync<TResult>(cancellationToken);
        }
    }
}
