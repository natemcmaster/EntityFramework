// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Tools.DotNet.Internal
{
    public class LoadContextExecutionStrategy : IExecutionStrategy
    {
        private readonly AssemblyLoadContext _context;
        private readonly string[] _args;
        public LoadContextExecutionStrategy([NotNull] AssemblyLoadContext context, [NotNull] string[] args)
        {
            Check.NotNull(context, nameof(context));
            Check.NotNull(args, nameof(args));

            _context = context;
            _args = args;
        }

        private const string ConsoleProgramType = "Microsoft.EntityFrameworkCore.Tools.Program";

        public int Execute()
        {
            var resolver = new EfConsoleCommandResolver();
            var consoleAssembly = _context.LoadFromAssemblyPath(resolver.FindEfCoreLibrary());
            var programType = consoleAssembly.GetType(ConsoleProgramType, throwOnError: true, ignoreCase: false);
            var mainMethodInfo = programType.GetTypeInfo().GetDeclaredMethods("Main").Single();
            return (int) mainMethodInfo.Invoke(null, new object[] { _args });
        }
    }
}