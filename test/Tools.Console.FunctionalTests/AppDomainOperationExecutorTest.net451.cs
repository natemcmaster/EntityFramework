// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET451
using System;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Relational.Design.Specification.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore.Tools.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tools.FunctionalTests
{
    public class AppDomainOperationExecutorTest : OperationExecutorTestBase
    {
        protected override IOperationExecutor CreateExecutorFromBuildResult(BuildFileResult build, string rootNamespace = null)
        {
            var setupInfo = new OperationExecutorSetup
            {
                AssemblyName = build.TargetName,
                StartupAssemblyName = build.TargetName,
                ContentRootPath = build.TargetDir,
                DataDirectory = build.TargetDir,
                ProjectDir = build.TargetDir,
                ApplicationBasePath = build.TargetDir,
                RootNamespace = rootNamespace,
            };

            return new AppDomainOperationExecutor(setupInfo,  null);
        }

        [Fact]
        public void Assembly_load_errors_are_wrapped()
        {
            var targetDir = AppDomain.CurrentDomain.BaseDirectory;
            var setupInfo = new OperationExecutorSetup
            {
                AssemblyName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location),
                StartupAssemblyName = "Unknown",
                ProjectDir = targetDir,
            };

            using (var executor = new AppDomainOperationExecutor(setupInfo, null))
            {
                Assert.Throws<OperationErrorException>(() => executor.GetContextTypes());
            }
        }
    }
}
#endif
