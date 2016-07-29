// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Design.Internal.Protocol;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class DesignClient
    {
        private readonly IOperationExecutor _executor;
        private readonly ICommunicationService _communicationService;
        private Task _runner;

        public DesignClient(ICommunicationService communicationService, IOperationExecutor executor)
        {
            _communicationService = communicationService;
            _executor = executor;
        }

        public bool Finished => _runner.IsCompleted;
        public Exception Error { get; private set; }
        public string AppId { get; set; }
        public int RetryTimeout { get; set; } = 3 * 1000;

        public virtual DesignClient Start(CancellationToken cancellationToken = default(CancellationToken))
        {
            Error = null;
            _runner = Task.Run(async () =>
                {
                    try
                    {
                        await RunAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        // unwrap
                        while (ex is AggregateException)
                            ex = ex.InnerException;

                        Error = ex;
                    }
                });
            return this;
        }

        public virtual DesignClient Wait()
        {
            Task.WaitAll(_runner);
            return this;
        }

        private async Task RunAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var settings = await GetDesignServerSettingsAsync(cancellationToken);
            if (settings == null)
            {
                throw new InvalidOperationException("Could not establish connection with design server");
            }

            await PumpOperations(settings.StayAlive, cancellationToken);
        }

        private async Task PumpOperations(bool stayAlive, CancellationToken cancellationToken)
        {
            var operations = new Queue<DesignOperation>();
            do
            {
                cancellationToken.ThrowIfCancellationRequested();
                while (operations.Any())
                {
                    var operation = operations.Dequeue();
                    var result = await ExecuteOperationAsync(operation, cancellationToken);
                    if (result.ServerRequestedClientShutdown)
                    {
                        return;
                    }
                }

                var newOperations = await GetPendingOperationsAsync(cancellationToken);

                if (newOperations?.Count > 0)
                {
                    foreach (var operation in newOperations)
                    {
                        operations.Enqueue(operation);
                    }
                }
                else if (stayAlive && RetryTimeout > 0)
                {
                    await Task.Delay(RetryTimeout);
                }
            }
            while (operations.Any() || stayAlive);
        }

        private Task<DesignServerSettings> GetDesignServerSettingsAsync(CancellationToken cancellationToken)
            => _communicationService.GetAsync<DesignServerSettings>(cancellationToken);

        private Task<ICollection<DesignOperation>> GetPendingOperationsAsync(CancellationToken cancellationToken)
            => _communicationService.GetAsync<ICollection<DesignOperation>>(cancellationToken);

        private async Task<OperationResultSubmitted> ExecuteOperationAsync(DesignOperation operation, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await _executor.ExecuteAsync(operation, cancellationToken);
            return await _communicationService.SendAsync<OperationResult, OperationResultSubmitted>(result, cancellationToken);
        }
    }
}
