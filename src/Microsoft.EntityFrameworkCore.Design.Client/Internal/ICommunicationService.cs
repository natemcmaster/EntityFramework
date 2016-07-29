// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public interface ICommunicationService
    {
        Task<T> GetAsync<T>(CancellationToken cancellationToken = default(CancellationToken));
        Task SendAsync<T>(T instance, CancellationToken cancellationToken = default(CancellationToken));
        Task<TResult> SendAsync<T, TResult>(T instance, CancellationToken cancellationToken = default(CancellationToken));
    }
}
