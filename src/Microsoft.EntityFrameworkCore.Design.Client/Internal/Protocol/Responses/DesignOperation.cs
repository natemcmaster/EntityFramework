// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using Microsoft.EntityFrameworkCore.Design.Internal.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Design.Internal.Protocol
{
    [Endpoint(Constants.ApiPrefix + "/operations", HttpMethodName.Get)]
    public class DesignOperation
    {
        public Guid Id { get; }
        public int Status { get; set; }
        public string Name { get; set; }
        public IDictionary Parameters { get; set; }
    }
}
