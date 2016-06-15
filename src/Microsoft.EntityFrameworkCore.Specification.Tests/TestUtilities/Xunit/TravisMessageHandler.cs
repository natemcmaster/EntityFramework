// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit
{
    /// <summary>
    ///     Logs a '.' for every test class to help keep Travis CI alive
    /// </summary>
    public class TravisMessageHandler : DefaultRunnerReporterMessageHandler
    {
        public TravisMessageHandler(IRunnerLogger logger)
            : base(logger)
        {
        }

        protected override bool Visit(ITestAssemblyExecutionStarting testAssemblyStarting)
        {
            lock (Logger.LockObject)
            {
                Logger.LogWarning("travis_fold:start:" + Path.GetFileNameWithoutExtension(testAssemblyStarting.Assembly.AssemblyFilename));
            }
            return base.Visit(testAssemblyStarting);
        }

        protected override bool Visit(ITestAssemblyExecutionFinished testAssemblyFinished)
        {
            lock (Logger.LockObject)
            {
                Logger.LogWarning("travis_fold:end:" + Path.GetFileNameWithoutExtension(testAssemblyFinished.Assembly.AssemblyFilename));
            }
            return base.Visit(testAssemblyFinished);
        }

        protected override bool Visit(ITestClassStarting testClassStarting)
        {
            lock (Logger.LockObject)
            {
                // Keeps Travis alive
                Logger.LogMessage(".");
            }

            return base.Visit(testClassStarting);
        }
    }
}
