// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Microsoft.EntityFrameworkCore.Internal;

// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleOther
namespace Microsoft.EntityFrameworkCore.Tools.Internal
{
    public class OperationExecutorFactory
    {
        public virtual OperationExecutorBase Create(CommandLineOptions options)
        {
            var appBasePath = Path.GetDirectoryName(options.Assembly);
            if (!Path.IsPathRooted(appBasePath))
            {
                appBasePath = Path.Combine(Directory.GetCurrentDirectory(), appBasePath);
            }

            var assemblyFileName = Path.GetFileNameWithoutExtension(options.Assembly);

            var setupInfo = new OperationExecutorSetup
            {
                AssemblyName = assemblyFileName,
                StartupAssemblyName = string.IsNullOrWhiteSpace(options.StartupAssembly)
                    ? assemblyFileName
                    : Path.GetFileNameWithoutExtension(options.StartupAssembly),
                ProjectDir = options.ProjectDirectory ?? Directory.GetCurrentDirectory(),
                DataDirectory = options.ProjectDirectory ?? Directory.GetCurrentDirectory(),
                ContentRootPath = options.ContentRootPath ?? appBasePath,
                RootNamespace = options.RootNamespace ?? assemblyFileName,
                EnvironmentName = options.EnvironmentName,
                ApplicationBasePath = appBasePath
            };

            try
            {
#if NET451
                if (!options.NoAppDomain)
                {
                    return new AppDomainOperationExecutor(setupInfo, options.AppConfigFile);
                }
#endif
                return new ReflectionOperationExecutor(setupInfo);
            }
            catch (FileNotFoundException ex) when (ex.FileName.StartsWith(OperationExecutorBase.DesignAssemblyName))
            {
                throw new OperationErrorException(ToolsStrings.DesignDependencyNotFound);
            }
        }
    }
}
