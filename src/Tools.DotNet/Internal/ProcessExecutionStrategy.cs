// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Tools.DotNet.Internal
{
    public class ProcessExecutionStrategy : IExecutionStrategy
    {
        private readonly CommandSpec _commandSpec;
        public ProcessExecutionStrategy([NotNull] CommandSpec commandSpec)
        {
            Check.NotNull(commandSpec, nameof(commandSpec));

            _commandSpec = commandSpec;
        }

        public int Execute()
        {
            return Command.Create(_commandSpec)
                .ForwardStdErr()
                .ForwardStdOut()
                .Execute()
                .ExitCode;
        }
    }
}