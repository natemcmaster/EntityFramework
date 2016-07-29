// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Design.Internal.Infrastructure;
using Microsoft.EntityFrameworkCore.Design.Internal.Protocol;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Design.Client.Tests
{
    public class HttpCommunicationServiceTest
    {
        [Fact]
        public async Task GetAsync()
        {
            var options = new DesignClientOptions { Server = new Uri("http://0.0.0.0:9000") };
            var handler = new MockHttpMessageHandler();
            var service = new MockHttpCommunicationService(options, handler);

            var endpoint = new Uri("http://0.0.0.0:9000" + Constants.ApiPrefix + "/settings");
            handler.QueueJsonResponse(
                endpoint,
                @"{
                ""stayAlive"": false
            }");

            var s = await service.GetAsync<DesignServerSettings>();
            Assert.Equal(1, handler.Requests[endpoint].Count);
            Assert.False(s.StayAlive);

            await Assert.ThrowsAsync<HttpRequestException>(async () => await service.GetAsync<DesignServerSettings>());
        }

        [Fact]
        public async Task GetAsyncThrowsIfEndpointAttributeMissing()
        {
            var options = new DesignClientOptions { Server = new Uri("http://0.0.0.0:9000") };
            var handler = new MockHttpMessageHandler();
            var service = new MockHttpCommunicationService(options, handler);

            await Assert.ThrowsAsync<ArgumentException>(async () => await service.GetAsync<HttpCommunicationServiceTest>());
        }

        [Endpoint(Constants.ApiPrefix + "/test-put", HttpMethodName.Put)]
        public class TestPutRequest
        {
        }

        [Fact]
        public async Task SendAsyncPut()
        {
            var options = new DesignClientOptions { Server = new Uri("http://0.0.0.0:9000") };
            var handler = new MockHttpMessageHandler();
            var service = new MockHttpCommunicationService(options, handler);
            var endpoint = new Uri("http://0.0.0.0:9000" + Constants.ApiPrefix + "/test-put");

            handler.QueueResponse(endpoint, new HttpResponseMessage(HttpStatusCode.OK));
            await service.SendAsync(new TestPutRequest());

            var message = Assert.Single(handler.Requests[endpoint]);
            Assert.Equal(HttpMethod.Put, message.Method);
        }

        public class TestResponse
        {
        }

        [Endpoint(Constants.ApiPrefix + "/test-post", HttpMethodName.Post)]
        public class TestPostRequest
        {
        }

        [Fact]
        public async Task SendAsyncPost()
        {
            var options = new DesignClientOptions { Server = new Uri("http://0.0.0.0:9000") };
            var handler = new MockHttpMessageHandler();
            var service = new MockHttpCommunicationService(options, handler);
            var endpoint = new Uri("http://0.0.0.0:9000" + Constants.ApiPrefix + "/test-post");

            handler.QueueResponse(endpoint, new HttpResponseMessage(HttpStatusCode.OK));
            await service.SendAsync(new TestPostRequest());

            var message = Assert.Single(handler.Requests[endpoint]);
            Assert.Equal(HttpMethod.Post, message.Method);
        }

        [Fact]
        public async Task SendAsyncWithResultThrowsIfResponseAttributeMissing()
        {
            var options = new DesignClientOptions { Server = new Uri("http://0.0.0.0:9000") };
            var handler = new MockHttpMessageHandler();
            var service = new MockHttpCommunicationService(options, handler);

            await Assert.ThrowsAsync<ArgumentException>(() => service.SendAsync<TestPostRequest, TestResponse>(new TestPostRequest()));
        }

        [Endpoint(Constants.ApiPrefix + "/test-get", HttpMethodName.Get)]
        public class TestGetRequest
        {
        }

        [Fact]
        public async Task SendAsyncMethodNotSupported()
        {
            var options = new DesignClientOptions { Server = new Uri("http://0.0.0.0:9000") };
            var handler = new MockHttpMessageHandler();
            var service = new MockHttpCommunicationService(options, handler);

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.SendAsync(new TestGetRequest()));
        }

        [Endpoint(Constants.ApiPrefix + "/test-post-bool", HttpMethodName.Post)]
        [Response(typeof(bool))]
        public class TestBoolRequest
        {
        }

        [Theory]
        [InlineData(HttpStatusCode.OK, true)]
        [InlineData(HttpStatusCode.Accepted, true)]
        [InlineData(HttpStatusCode.Created, true)]
        [InlineData(HttpStatusCode.Ambiguous, false)]
        [InlineData(HttpStatusCode.NotFound, false)]
        [InlineData(HttpStatusCode.InternalServerError, false)]
        public async Task SendAsyncWithBoolResult(HttpStatusCode code, bool success)
        {
            var options = new DesignClientOptions { Server = new Uri("http://0.0.0.0:9000") };
            var handler = new MockHttpMessageHandler();
            var service = new MockHttpCommunicationService(options, handler);

            handler.QueueResponse(new Uri("http://0.0.0.0:9000" + Constants.ApiPrefix + "/test-post-bool"), new HttpResponseMessage(code));
            Assert.Equal(success, await service.SendAsync<TestBoolRequest, bool>(new TestBoolRequest()));
        }
    }

    public class MockHttpCommunicationService : HttpCommunicationService
    {
        public MockHttpCommunicationService(DesignClientOptions options, MockHttpMessageHandler handler)
            : base(options, handler)
        {
            _handler = handler;
        }

        private MockHttpMessageHandler _handler;
    }

    public class MockHttpMessageHandler : DelegatingHandler
    {
        private readonly Dictionary<Uri, Queue<HttpResponseMessage>> _responseQueue = new Dictionary<Uri, Queue<HttpResponseMessage>>();
        public Dictionary<Uri, List<HttpRequestMessage>> Requests { get; } = new Dictionary<Uri, List<HttpRequestMessage>>();

        public void QueueJsonResponse(Uri uri, string json)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            QueueResponse(uri, response);
        }

        public void QueueResponse(Uri uri, HttpResponseMessage m)
        {
            Queue<HttpResponseMessage> queue;
            if (!_responseQueue.TryGetValue(uri, out queue))
            {
                queue = new Queue<HttpResponseMessage>();
                _responseQueue[uri] = queue;
            }
            queue.Enqueue(m);
        }

        private static readonly HttpResponseMessage s_notFound = new HttpResponseMessage(HttpStatusCode.NotFound);

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            List<HttpRequestMessage> list;
            if (!Requests.TryGetValue(request.RequestUri, out list))
            {
                list = new List<HttpRequestMessage>();
                Requests[request.RequestUri] = list;
            }
            list.Add(request);

            Queue<HttpResponseMessage> queue;
            if (!_responseQueue.TryGetValue(request.RequestUri, out queue)
                || queue.Count == 0)
            {
                return Task.FromResult(s_notFound);
            }
            return Task.FromResult(queue.Dequeue());
        }
    }
}
