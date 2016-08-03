// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Tools.Internal
{
    public class OperationExecutorSetup
    {
        public string ProjectDir { get; set; }

        public string ContentRootPath { get; set; }

        public string DataDirectory { get; set; }

        public string RootNamespace { get; set; }

        public string EnvironmentName { get; set; }
        public string ApplicationBasePath { get; set; }
        public string AssemblyName { get; set; }
        public string StartupAssemblyName { get; set; }
    }
}
