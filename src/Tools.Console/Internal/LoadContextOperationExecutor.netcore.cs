// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Tools.Internal
{
    public class LoadContextOperationExecutor : OperationExecutorBase
    {
        public LoadContextOperationExecutor([NotNull] OperationExecutorSetup setupInfo)
            : base(setupInfo)
        {
        }

        protected override object CreateResultHandler()
        {
            throw new System.NotImplementedException();
        }

        protected override void Execute(string operationName, object resultHandler, IDictionary arguments)
        {
            throw new System.NotImplementedException();
        }
        public override void Dispose()
        {
        }
    }
}