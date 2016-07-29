// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Design.Internal.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Design.Internal.Protocol
{
    [Endpoint(Constants.ApiPrefix + "/operation-results", HttpMethodName.Put)]
    [Response(typeof(OperationResultSubmitted))]
    public class OperationResult
    {
        public Guid Id { get; }
        public IDictionary<string, string> Results { get; set; }
    }
}
