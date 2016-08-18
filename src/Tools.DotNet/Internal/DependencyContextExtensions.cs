// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.DotNet.InternalAbstractions;
using Microsoft.Extensions.DependencyModel;

namespace Microsoft.EntityFrameworkCore.Tools.DotNet.Internal
{
    public static class DependencyContextExtensions
    {
        /// <summary>
        ///     Creates a load context that immitates corehost
        ///     <seealso href="https://github.com/dotnet/cli/blob/rel/1.0.0/Documentation/specs/corehost.md" />
        /// </summary>
        /// <param name="dependencyContext"></param>
        /// <returns></returns>
        public static AssemblyLoadContext CreateLoadContext(this IProjectContext projectContext)
        {
            // see https://github.com/dotnet/cli/blob/rel/1.0.0/Documentation/specs/corehost.md
            // 1. servicing cache
            // 2. app-local
            // 3. nuget cache(s)

            DependencyContext depContext;
            using (var fileStream = new FileStream(projectContext.DepsJson, FileMode.Open))
            {
                depContext = new DependencyContextJsonReader().Read(fileStream);
            }

            var ridGraph = depContext.RuntimeGraph.Any()
                ? depContext.RuntimeGraph
                : DependencyContext.Default.RuntimeGraph;

            var fallbackGraph = ridGraph.First(g => g.Runtime == RuntimeEnvironment.GetRuntimeIdentifier());

            var searchPaths = new[]
            {
                // TODO servicing cache
                projectContext.TargetDirectory,
            };

            var managedLibraries = depContext.ResolveRuntimeAssemblies(projectContext.PackagesDirectory, fallbackGraph).ToDictionary(r => r.Name, r => r.Path);
            var nativeLibraries = depContext.ResolveNativeAssets(projectContext.PackagesDirectory, fallbackGraph).ToDictionary(n => n.Name, n => n.Path);

            return new DesignLoadContext(managedLibraries, nativeLibraries, searchPaths);
        }

        private static IEnumerable<Asset> ResolveRuntimeAssemblies(this DependencyContext depContext, string packageDir, RuntimeFallbacks runtimeGraph)
        {
            var rids = GetRids(runtimeGraph);
            return from library in depContext.RuntimeLibraries
                   from assetPath in SelectAssets(rids, library.RuntimeAssemblyGroups)
                   select Asset.Create(packageDir, library.Name, library.Version, assetPath);
        }

        public static IEnumerable<Asset> ResolveNativeAssets(this DependencyContext depContext,
            string packageDir,
            RuntimeFallbacks runtimeGraph)
        {
            return Enumerable.Empty<Asset>();
        }

        private static IEnumerable<string> GetRids(RuntimeFallbacks runtimeGraph)
        {
            return Enumerable.Concat(new[] { runtimeGraph.Runtime }, runtimeGraph?.Fallbacks ?? Enumerable.Empty<string>());
        }

        private static IEnumerable<string> SelectAssets(IEnumerable<string> rids, IEnumerable<RuntimeAssetGroup> groups)
        {
            foreach (var rid in rids)
            {
                var group = groups.FirstOrDefault(g => g.Runtime == rid);
                if (group != null)
                {
                    return group.AssetPaths;
                }
            }

            // Return the RID-agnostic group
            return groups.GetDefaultAssets();
        }

        public class Asset
        {
            public Asset(string name, string path)
            {
                Name = name;
                Path = path;
            }

            public string Name { get; }
            public string Path { get; }

            public static Asset Create(string packageDir, string libraryName, string version, string path)
            {
                var name = System.IO.Path.GetFileNameWithoutExtension(path);
                return new Asset(name, System.IO.Path.Combine(packageDir, libraryName, version, path));
            }
        }
    }

}
