// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using NuGet.Frameworks;

namespace Microsoft.EntityFrameworkCore.Tools.DotNet.Internal
{
    public class EfConsoleExecutionStrategyFactory
    {
        private readonly EfConsoleCommandResolver _resolver;

        public EfConsoleExecutionStrategyFactory([NotNull] EfConsoleCommandResolver resolver)
        {
            Check.NotNull(resolver, nameof(resolver));

            _resolver = resolver;
        }

        public virtual IExecutionStrategy Create(IProjectContext startupProject, IProjectContext targetProject, bool verbose, IEnumerable<string> additionalArguments)
        {
            if (startupProject.IsClassLibrary)
            {
                if (!NuGetFrameworkUtility.IsCompatibleWithFallbackCheck(FrameworkConstants.CommonFrameworks.NetCoreApp10, startupProject.TargetFramework))
                {
                    throw new OperationErrorException("dotnet-ef cannot load this class library project because it is not compatible with .NET Core");
                }

                return CreateLoadContextStrategy(startupProject, targetProject, verbose, additionalArguments);
            }
            else
            {
                return CreateProcessStrategy(startupProject, targetProject, verbose, additionalArguments);
            }
        }

        private IExecutionStrategy CreateLoadContextStrategy(IProjectContext startupProject, IProjectContext targetProject, bool verbose, IEnumerable<string> additionalArguments)
        {
            var loadContext = startupProject.CreateLoadContext();
            var args = CreateArgs(startupProject, targetProject, verbose)
                .Concat(additionalArguments)
                .ToArray();
            return new LoadContextExecutionStrategy(loadContext, args);
        }

        private IExecutionStrategy CreateProcessStrategy(IProjectContext startupProject, IProjectContext targetProject, bool verbose, IEnumerable<string> additionalArguments)
        {
            var args = CreateArgs(startupProject, targetProject, verbose);
            CommandSpec commandSpec;
            if (startupProject.TargetFramework.IsDesktop())
            {
                if (startupProject.Config != null)
                {
                    args = args.Concat(new [] { ConfigOptionTemplate, startupProject.Config });
                }
                args = args.Concat(additionalArguments);
                commandSpec = ResolveDesktopCommand(startupProject, args);
            }
            else
            {
                args = args.Concat(additionalArguments);
                commandSpec = ResolveDotNetCommand(startupProject, args);
            }

            return new ProcessExecutionStrategy(commandSpec);
        }

        private CommandSpec ResolveDesktopCommand(IProjectContext startupProject, IEnumerable<string> args)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new OperationErrorException(ToolsDotNetStrings.DesktopCommandsRequiresWindows(startupProject.TargetFramework));
            }

            var arguments = new ResolverArguments
            {
                IsDesktop = true,
                CommandArguments = args,
                NuGetPackageRoot = startupProject.PackagesDirectory
            };

            return _resolver.Resolve(arguments);
        }

        private CommandSpec ResolveDotNetCommand(IProjectContext startupProject, IEnumerable<string> args)
        {
            if (!File.Exists(startupProject.DepsJson))
            {
                throw new OperationErrorException(ToolsDotNetStrings.MissingDepsJsonFile(startupProject.DepsJson));
            }

            var arguments = new ResolverArguments
            {
                IsDesktop = false,
                CommandArguments = args,
                DepsJsonFile = startupProject.DepsJson,
                RuntimeConfigJson = startupProject.RuntimeConfigJson,
                NuGetPackageRoot = startupProject.PackagesDirectory
            };

            return _resolver.Resolve(arguments);
        }

        private const string ConfigOptionTemplate = "--config";
        private const string AssemblyOptionTemplate = "--assembly";
        private const string StartupAssemblyOptionTemplate = "--startup-assembly";
        private const string DataDirectoryOptionTemplate = "--data-dir";
        private const string ProjectDirectoryOptionTemplate = "--project-dir";
        private const string ContentRootPathOptionTemplate = "--content-root-path";
        private const string RootNamespaceOptionTemplate = "--root-namespace";
        private const string VerboseOptionTemplate = "--verbose";
        private IEnumerable<string> CreateArgs(IProjectContext startupProject, IProjectContext targetProject, bool verbose)
        {
            var targetAssembly = targetProject.ProjectFullPath.Equals(startupProject.ProjectFullPath)
                ? startupProject.AssemblyFullPath
                // This assumes the target assembly is present in the startup project context build output folder
                : Path.Combine(startupProject.TargetDirectory, Path.GetFileName(targetProject.AssemblyFullPath));

            return new[]
            {
                AssemblyOptionTemplate, targetAssembly,
                StartupAssemblyOptionTemplate, startupProject.AssemblyFullPath,
                DataDirectoryOptionTemplate,startupProject.TargetDirectory,
                ProjectDirectoryOptionTemplate, Path.GetDirectoryName(targetProject.ProjectFullPath),
                ContentRootPathOptionTemplate, Path.GetDirectoryName(startupProject.ProjectFullPath),
                RootNamespaceOptionTemplate, targetProject.RootNamespace,
            }
            .Concat(verbose
                ? new[] { VerboseOptionTemplate }
                : Enumerable.Empty<string>());
        }

    }
}
