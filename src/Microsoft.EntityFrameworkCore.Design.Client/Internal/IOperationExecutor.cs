// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Design.Internal.Protocol;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public interface IOperationExecutor
    {
        Task<OperationResult> ExecuteAsync(DesignOperation operation, CancellationToken cancellationToken = default(CancellationToken));
    }
}
