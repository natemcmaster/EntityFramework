// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Design.Internal.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    internal sealed class ResponseAttribute : Attribute
    {
        public ResponseAttribute(Type responseType)
        {
            ResponseType = responseType;
        }

        public Type ResponseType { get; set; }
    }
}
