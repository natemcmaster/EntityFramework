using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.InternalAbstractions;
using Microsoft.DotNet.ProjectModel;
using Microsoft.EntityFrameworkCore.Design.Loader;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using NuGet.Frameworks;

namespace Microsoft.EntityFrameworkCore.Tools.Internal
{
    public class AssemblyLoadContextOperationExecutor : OperationExecutorBase
    {
        private readonly Assembly _commandsAssembly;
        private readonly object _executor;
        private readonly string _projectDir;

        public AssemblyLoadContextOperationExecutor([CanBeNull] string targetProjectPath,
            [CanBeNull] string startupProjectFile,
            [CanBeNull] string configuration,
            [CanBeNull] string buildBasePath,
            [CanBeNull] string outputPath,
            [CanBeNull] NuGetFramework startupFramework,
            [CanBeNull] string environmentName,
            bool noBuild)
        {
            Reporter.Output("Executor json assembly version = " + typeof(JsonConvert).GetTypeInfo().Assembly.GetName().Version);
            targetProjectPath = targetProjectPath ?? Directory.GetCurrentDirectory();

            Project targetProject;
            if (!ProjectReader.TryGetProject(targetProjectPath, out targetProject))
            {
                throw new OperationErrorException($"Could not load target project '{targetProjectPath}'");
            }

            Reporter.Verbose(ToolsStrings.LogUsingTargetProject(targetProject.Name));

            Project startupProject;
            if (startupProjectFile != null)
            {
                if (!ProjectReader.TryGetProject(startupProjectFile, out startupProject))
                {
                    throw new OperationErrorException($"Could not load project '{startupProjectFile}'");
                }
            }
            else
            {
                startupProject = targetProject;
            }

            Reporter.Verbose(ToolsStrings.LogUsingStartupProject(startupProject.Name));

            if (startupFramework == null)
            {
                var frameworks = startupProject.GetTargetFrameworks().Select(i => i.FrameworkName);
                startupFramework = NuGetFrameworkUtility.GetNearest(frameworks,
                    FrameworkConstants.CommonFrameworks.NetCoreApp10, f => f)
                                   ?? frameworks.FirstOrDefault();

                Reporter.Verbose(ToolsStrings.LogUsingFramework(startupFramework.GetShortFolderName()));
            }

            if (configuration == null)
            {
                configuration = Constants.DefaultConfiguration;

                Reporter.Verbose(ToolsStrings.LogUsingConfiguration(configuration));
            }

            if (!noBuild)
            {
                var buildExitCode = BuildCommandFactory.Create(
                    startupProject.ProjectFilePath,
                    configuration,
                    startupFramework,
                    buildBasePath,
                    outputPath)
                    .ForwardStdErr()
                    .ForwardStdOut()
                    .Execute()
                    .ExitCode;
                if (buildExitCode != 0)
                {
                    throw new OperationErrorException(ToolsStrings.BuildFailed(startupProject.Name));
                }
            }
            var startupProjectContext = ProjectContext.Create(
                startupProject.ProjectFilePath,
                startupFramework,
                RuntimeEnvironmentRidExtensions.GetAllCandidateRuntimeIdentifiers());

            var isExecutable = startupProject.GetCompilerOptions(startupFramework, configuration).EmitEntryPoint
                               ??
                               startupProject.GetCompilerOptions(null, configuration).EmitEntryPoint.GetValueOrDefault();


            var startupAssembly = startupProject.GetCompilerOptions(startupFramework, configuration).OutputName;

            var targetAssembly = targetProject.ProjectFilePath.Equals(startupProject.ProjectFilePath)
                ? startupAssembly
                // This assumes the target assembly is present in the startup project context and is a *.dll
                // TODO create a project context for target project as well to ensure filename is correct
                : targetProject.GetCompilerOptions(null, configuration).OutputName;

            _projectDir = targetProject.ProjectDirectory;
            var assemblyLoadContext = startupProjectContext.CreateLoadContext(RuntimeEnvironment.GetRuntimeIdentifier(), configuration);

            try
            {
                _commandsAssembly = assemblyLoadContext.LoadFromAssemblyName(
                    new AssemblyName(DesignAssemblyName));
            }
            catch (FileNotFoundException ex)
            {
                Reporter.Verbose(ex.ToString());

                throw new OperationErrorException(
                    "Cannot execute this command because " + DesignAssemblyName + " is not installed in the " +
                    "startup project '" + startupProject.Name + "'.");
            }

            var assemblyLoader = Activator.CreateInstance(
                _commandsAssembly.GetType(
                    "Microsoft.EntityFrameworkCore.Design.AssemblyLoader",
                    true,
                    false),
                (Func<AssemblyName, Assembly>) assemblyLoadContext.LoadFromAssemblyName);

            var logHandler = Activator.CreateInstance(
                _commandsAssembly.GetType(
                    "Microsoft.EntityFrameworkCore.Design.OperationLogHandler",
                    true,
                    false),
                (Action<string>) Reporter.Error,
                (Action<string>) Reporter.Warning,
                (Action<string>) Reporter.Output,
                (Action<string>) Reporter.Verbose,
                (Action<string>) Reporter.Verbose);

            _executor = Activator.CreateInstance(
                _commandsAssembly.GetType(ExecutorTypeName, true, false),
                logHandler,
                new Dictionary<string, string>
                {
                    ["targetName"] = targetAssembly,
                    ["startupTargetName"] = startupAssembly,
                    ["environment"] = environmentName,
                    ["projectDir"] = targetProject.ProjectDirectory,
                    ["contentRootPath"] = startupProject.ProjectDirectory,
                    ["rootNamespace"] = targetProject.Name
                },
                assemblyLoader);
        }

        public override void Dispose()
        {
        }


        protected override object InvokeOperationImpl(string operationName, IDictionary arguments, bool isVoid = false)
        {
            var resultHandler = (dynamic) Activator.CreateInstance(
                _commandsAssembly.GetType(
                    "Microsoft.EntityFrameworkCore.Design.OperationResultHandler",
                    true,
                    false));

            var currentDirectory = Directory.GetCurrentDirectory();

            Reporter.Verbose("Using current directory '" + _projectDir + "'.");

            Directory.SetCurrentDirectory(_projectDir);
            try
            {
                Activator.CreateInstance(
                    _commandsAssembly.GetType(ExecutorTypeName + "+" + operationName, true, true),
                    _executor,
                    resultHandler,
                    arguments);
            }
            finally
            {
                Directory.SetCurrentDirectory(currentDirectory);
            }

            if (resultHandler.ErrorType != null)
            {
                if (resultHandler.ErrorType == OperationExceptionTypeName)
                {
                    Reporter.Verbose(resultHandler.ErrorStackTrace);
                }
                else
                {
                    Reporter.Output(resultHandler.ErrorStackTrace);
                }
                throw new OperationErrorException(resultHandler.ErrorMessage);
            }
            return resultHandler.HasResult
                ? resultHandler.Result
                : null;
        }
    }
}