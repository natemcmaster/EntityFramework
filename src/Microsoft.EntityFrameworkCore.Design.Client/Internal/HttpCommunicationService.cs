// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Design.Internal.Infrastructure;
using Newtonsoft.Json;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class HttpCommunicationService : ICommunicationService
    {
        private static readonly MediaTypeWithQualityHeaderValue s_jsonMedia
            = MediaTypeWithQualityHeaderValue.Parse("application/json");

        private readonly HttpClient _httpClient;

        public HttpCommunicationService(DesignClientOptions options)
            : this(options, new HttpClientHandler())
        {
        }

        // For testing
        protected HttpCommunicationService(DesignClientOptions options, HttpMessageHandler handler)
        {
            _httpClient = new HttpClient(handler)
            {
                BaseAddress = options.Server,
                Timeout = TimeSpan.FromSeconds(30)
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(s_jsonMedia);
        }

        private EndpointAttribute GetEndpoint<T>()
        {
            // TODO cache
            var attr = typeof(T).GetTypeInfo().GetCustomAttribute<EndpointAttribute>();
            if (attr == null)
                throw new ArgumentException(nameof(T), "Could not identify the " + nameof(EndpointAttribute) + " on type " + typeof(T).Name);
            return attr;
        }

        public async Task<T> GetAsync<T>(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var endpoint = GetEndpoint<T>();

            if (endpoint.Method != HttpMethodName.Get)
                throw new ArgumentException(nameof(T), "Endpoint for type " + typeof(T).Name + " does not support 'Get'");

            var m = await _httpClient.GetAsync(endpoint.Url, cancellationToken);

            m.EnsureSuccessStatusCode();

            return await Deserialize<T>(m);
        }

        public async Task SendAsync<T>(T instance, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            HttpResponseMessage m;

            using (var stream = Serialize(instance))
            {
                m = await InternalSendAsync(instance, stream, cancellationToken);
            }

            m.EnsureSuccessStatusCode();
        }

        public async Task<TResult> SendAsync<T, TResult>(T instance, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var responseType = typeof(T).GetTypeInfo().GetCustomAttribute<ResponseAttribute>()?.ResponseType;
            if (responseType != typeof(TResult))
                throw new ArgumentException(nameof(TResult), "Invalid result type '" + typeof(TResult).Name + "' for request type " + typeof(T).Name);

            HttpResponseMessage m;
            using (var stream = Serialize(instance))
            {
                m = await InternalSendAsync(instance, stream, cancellationToken);
            }

            if (typeof(TResult) == typeof(bool))
            {
                return (TResult)(object)m.IsSuccessStatusCode;
            }

            m.EnsureSuccessStatusCode();

            return await Deserialize<TResult>(m);
        }

        private Stream Serialize<T>(T instance)
        {
            var stream = new MemoryStream();

            using (var sw = new StreamWriter(stream))
                using (var writer = new JsonTextWriter(sw))
                {
                    var serializier = new JsonSerializer();
                    serializier.Serialize(writer, instance);
                }

            return stream;
        }

        private async Task<T> Deserialize<T>(HttpResponseMessage m)
        {
            using (var stream = await m.Content.ReadAsStreamAsync())
                using (var sr = new StreamReader(stream))
                    using (var reader = new JsonTextReader(sr))
                    {
                        var serializier = new JsonSerializer();

                        return serializier.Deserialize<T>(reader);
                    }
        }

        private Task<HttpResponseMessage> InternalSendAsync<T>(T instance, Stream stream, CancellationToken ct)
        {
            var endpoint = GetEndpoint<T>();

            var content = new StreamContent(stream);
            content.Headers.ContentType = s_jsonMedia;
            switch (endpoint.Method)
            {
                case HttpMethodName.Post:
                    return _httpClient.PostAsync(endpoint.Url, content, ct);
                case HttpMethodName.Put:
                    return _httpClient.PutAsync(endpoint.Url, content, ct);

                default:
                    throw new ArgumentOutOfRangeException(nameof(T), "SendAsync does not support endpoints with method " + endpoint.Method);
            }
        }
    }
}
