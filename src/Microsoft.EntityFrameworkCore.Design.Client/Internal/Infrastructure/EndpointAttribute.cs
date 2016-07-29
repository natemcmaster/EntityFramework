// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Design.Internal.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    internal sealed class EndpointAttribute : Attribute
    {
        public string Url { get; set; }
        public HttpMethodName Method { get; set; }

        public EndpointAttribute(string url, HttpMethodName method)
        {
            Url = url;
            Method = method;
        }

        public EndpointAttribute(string url)
        {
            Url = url;
        }

        public EndpointAttribute()
        {
        }
    }
}
