// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design;

namespace Microsoft.EntityFrameworkCore.Tools.Internal
{
    public abstract class OperationExecutorBase : IOperationExecutor
    {
        private const string DataDirEnvName = "ADONET_DATA_DIR";
        protected const string DesignAssemblyName = "Microsoft.EntityFrameworkCore.Design";
        protected const string ExecutorTypeName = "Microsoft.EntityFrameworkCore.Design.OperationExecutor";
        protected const string OperationExceptionTypeName = "Microsoft.EntityFrameworkCore.Design.OperationException";

        public static IDictionary EmptyArguments = new Dictionary<string, object>();

        //protected OperationExecutorBase(
        //    [CanBeNull] string dataDirectory)
        //{
        //    if (!string.IsNullOrEmpty(dataDirectory))
        //    {
        //        Environment.SetEnvironmentVariable(DataDirEnvName, dataDirectory);
        //    }
        //}

        public abstract void Dispose();
        private TResult InvokeOperation<TResult>(string operation)
            => InvokeOperation<TResult>(operation, EmptyArguments);

        private TResult InvokeOperation<TResult>(string operation, IDictionary arguments)
            => (TResult)InvokeOperationImpl(operation, arguments);

        private void InvokeOperation(string operation, IDictionary arguments)
            => InvokeOperationImpl(operation, arguments, true);

        protected abstract object InvokeOperationImpl(string operationName, IDictionary arguments, bool isVoid = false);

        public IDictionary AddMigration(string name, string outputDir, string contextType)
            => InvokeOperation<IDictionary>("AddMigration",
                new Dictionary<string, string>
                {
                    ["name"] = name,
                    ["outputDir"] = outputDir,
                    ["contextType"] = contextType
                });

        public void RemoveMigration(string contextType, bool force)
            => InvokeOperation(
                "RemoveMigration",
                new Dictionary<string, object>
                {
                    ["contextType"] = contextType,
                    ["force"] = force
                });

        public IEnumerable<IDictionary> GetMigrations(string contextType)
            => InvokeOperation<IEnumerable<IDictionary>>(
                "GetMigrations",
                new Dictionary<string, object>
                {
                    ["contextType"] = contextType
                });

        public void DropDatabase(string contextType)
            => InvokeOperation(
                "DropDatabase",
                new Dictionary<string, object>
                {
                    ["contextType"] = contextType
                });

        public IDictionary GetDatabase(string name)
            => InvokeOperation<IDictionary>(
                "GetDatabase",
                new Dictionary<string, object>
                {
                    ["contextType"] = name
                });

        public void UpdateDatabase(string migration, string contextType)
            => InvokeOperation("UpdateDatabase",
                new Dictionary<string, string>
                {
                    ["targetMigration"] = migration,
                    ["contextType"] = contextType
                });

        public IEnumerable<IDictionary> GetContextTypes()
            => InvokeOperation<IEnumerable<IDictionary>>("GetContextTypes");

        public IEnumerable<string> ReverseEngineer(string provider,
            string connectionString,
            string outputDir,
            string dbContextClassName,
            IEnumerable<string> schemaFilters,
            IEnumerable<string> tableFilters,
            bool useDataAnnotations,
            bool overwriteFiles)
            => InvokeOperation<IEnumerable<string>>("ReverseEngineer",
                new Dictionary<string, object>
                {
                    ["provider"] = provider,
                    ["connectionString"] = connectionString,
                    ["outputDir"] = outputDir,
                    ["dbContextClassName"] = dbContextClassName,
                    ["schemaFilters"] = schemaFilters,
                    ["tableFilters"] = tableFilters,
                    ["useDataAnnotations"] = useDataAnnotations,
                    ["overwriteFiles"] = overwriteFiles
                });

        public string ScriptMigration(
            string fromMigration,
            string toMigration,
            bool idempotent,
            string contextType)
            => InvokeOperation<string>(
                "ScriptMigration",
                new Dictionary<string, object>
                {
                    ["fromMigration"] = fromMigration,
                    ["toMigration"] = toMigration,
                    ["idempotent"] = idempotent,
                    ["contextType"] = contextType
                });

        public string GetContextType(string name)
            => InvokeOperation<string>(
                "GetContextType",
                new Dictionary<string, string>
                {
                    ["name"] = name
                });
    }
}
