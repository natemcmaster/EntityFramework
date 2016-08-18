// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace Microsoft.EntityFrameworkCore.Tools.DotNet.Internal
{
    internal sealed class DesignLoadContext : AssemblyLoadContext
    {
        private readonly IDictionary<string, string> _assemblyPaths;
        private readonly IDictionary<string, string> _nativeLibraries;
        private readonly IEnumerable<string> _searchPaths;

        private static readonly string[] NativeLibraryExtensions;
        private static readonly string[] ManagedAssemblyExtensions = new[]
        {
            ".dll",
            ".ni.dll",
            ".exe",
            ".ni.exe"
        };


        static DesignLoadContext()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                NativeLibraryExtensions = new[] { ".dll" };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                NativeLibraryExtensions = new[] { ".dylib" };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                NativeLibraryExtensions = new[] { ".so" };
            }
            else
            {
                NativeLibraryExtensions = Array.Empty<string>();
            }
        }

        public DesignLoadContext(IDictionary<string, string> assemblyPaths,
                                  IDictionary<string, string> nativeLibraries,
                                  IEnumerable<string> searchPaths)
        {
            _assemblyPaths = assemblyPaths;
            _nativeLibraries = nativeLibraries;
            _searchPaths = searchPaths;
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            string path;
            if (SearchForLibrary(ManagedAssemblyExtensions, assemblyName.Name, out path) || _assemblyPaths.TryGetValue(assemblyName.Name, out path))
            {
                return LoadFromAssemblyPath(path);
            }

            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string path;
            if (SearchForLibrary(NativeLibraryExtensions, unmanagedDllName, out path) || _nativeLibraries.TryGetValue(unmanagedDllName, out path))
            {
                return LoadUnmanagedDllFromPath(path);
            }

            return base.LoadUnmanagedDll(unmanagedDllName);
        }

        private bool SearchForLibrary(string[] extensions, string name, out string path)
        {
            foreach (var searchPath in _searchPaths)
            {
                foreach (var extension in extensions)
                {
                    var candidate = Path.Combine(searchPath, name + extension);
                    if (File.Exists(candidate))
                    {
                        path = candidate;
                        return true;
                    }
                }
            }
            path = null;
            return false;
        }
    }
}
