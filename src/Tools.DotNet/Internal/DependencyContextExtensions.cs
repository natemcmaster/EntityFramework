// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.EntityFrameworkCore.Tools.DotNet.Internal;

namespace Microsoft.Extensions.DependencyModel
{
    public static class DependencyContextExtensions
    {
        /// <summary>
        ///     Creates a load context that immitates corehost
        ///     <seealso href="https://github.com/dotnet/cli/blob/rel/1.0.0/Documentation/specs/corehost.md" />
        /// </summary>
        /// <param name="dependencyContext"></param>
        /// <returns></returns>
        public static AssemblyLoadContext CreateLoadContext(this DependencyContext dependencyContext)
        {
            // see https://github.com/dotnet/cli/blob/rel/1.0.0/Documentation/specs/corehost.md
            // 1. servicing cache
            // 2. app-local
            // 3. nuget cache(s)

            return new DesignLoadContext();
        }

        public static IEnumerable<RuntimeAssembly> ResolveRuntimeAssemblies(this DependencyContext depContext, RuntimeFallbacks runtimeGraph)
        {
            return from library in depContext.RuntimeLibraries
                   from assetPath in ResolveAssets(runtimeGraph, library.RuntimeAssemblyGroups)
                   select RuntimeAssembly.Create(assetPath);
        }

        public static IEnumerable<NativeAsset> ResolveNativeAssets(this DependencyContext depContext,
            RuntimeFallbacks runtimeGraph)
        {
            return Enumerable.Empty<NativeAsset>();
        }

        private static IEnumerable<string> ResolveAssets(
            RuntimeFallbacks runtimeGraph,
            IEnumerable<RuntimeAssetGroup> assets)
        {
            var rids = Enumerable.Concat(new[] { runtimeGraph.Runtime }, runtimeGraph?.Fallbacks ?? Enumerable.Empty<string>());
            return SelectAssets(rids, assets);
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
    }

    public class NativeAsset
    {
        public NativeAsset(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public string Name { get; }
        public string Path { get; }

        public static NativeAsset Create(string path)
        {
            var name = System.IO.Path.GetFileNameWithoutExtension(path);
            return new NativeAsset(name, path);
        }
    }
}
