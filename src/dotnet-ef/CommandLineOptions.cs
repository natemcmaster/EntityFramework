// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.CommandLineUtils;
using System;
using Microsoft.DotNet.Cli.Utils;
using NuGet.Frameworks;

// ReSharper disable ArgumentsStyleLiteral
namespace Microsoft.EntityFrameworkCore.Tools
{
    public class CommandLineOptions
    {
        public ICommand Command { get; set; }
        public bool IsHelp { get; set; }
        public bool Verbose { get; set; }
        public string EnvironmentName { get; set; }
     

        public static CommandLineOptions Parse(params string[] args)
        {
            var options = new CommandLineOptions();

            var app = new CommandLineApplication
            {
                Name = "dotnet ef",
                FullName = "Entity Framework Core .NET CLI Commands"
            };

            app.HelpOption();
            app.VersionOption(Program.GetVersion);
            var verbose = app.Option("--verbose", "Show verbose output", inherited: true);

            var targetProjectOption = app.Option(
                "-p|--project <PROJECT>",
                "The project to target (defaults to the project in the current directory). Can be a path to a project.json or a project directory.");
            var startupProjectOption = app.Option(
                "-s|--startup-project <PROJECT>",
                "The path to the project containing Startup (defaults to the target project). Can be a path to a project.json or a project directory.",
                inherited: true);
            var configurationOption = app.Option(
                "-c|--configuration <CONFIGURATION>",
                $"Configuration under which to load (defaults to {Constants.DefaultConfiguration})");
            var frameworkOption = app.Option(
                "-f|--framework <FRAMEWORK>",
                $"Target framework to load from the startup project (defaults to the framework most compatible with {FrameworkConstants.CommonFrameworks.NetCoreApp10}).");
            var buildBasePathOption = app.Option(
                "-b|--build-base-path <OUTPUT_DIR>",
                "Directory in which to find temporary outputs.");
            var outputOption = app.Option(
                "-o|--output <OUTPUT_DIR>",
                "Directory in which to find outputs");
            var noBuildOption = app.Option("--no-build", "Do not build before executing.", inherited: true);

            var environment = app.Option(
                "-e|--environment <environment>",
                "The environment to use. If omitted, \"Development\" is used.", inherited: true);


            app.Command("database", command =>
                {
                    command.Description = "Commands to manage your database";
                    command.HelpOption();

                    command.Command("update", c => DatabaseUpdateCommand.ParseOptions(c, options));
                    command.Command("drop", c => DatabaseDropCommand.ParseOptions(c, options));
                    command.OnExecute(() =>
                    {
                        WriteLogo();
                        app.ShowHelp("database");
                    });
                });

            app.Command("dbcontext", command =>
                {
                    command.Description = "Commands to manage your DbContext types";
                    command.HelpOption();

                    command.Command("list", c => DbContextListCommand.ParseOptions(c, options));
                    command.Command("scaffold", c => DbContextScaffoldCommand.ParseOptions(c, options));
                    command.OnExecute(() =>
                    {
                        WriteLogo();
                        command.ShowHelp();
                    });
                });

            app.Command("migrations", command =>
                {
                    command.Description = "Commands to manage your migrations";
                    command.HelpOption();

                    command.Command("add", c => MigrationsAddCommand.ParseOptions(c, options));
                    command.Command("list", c => MigrationsListCommand.ParseOptions(c, options));
                    command.Command("remove", c => MigrationsRemoveCommand.ParseOptions(c, options));
                    command.Command("script", c => MigrationsScriptCommand.ParseOptions(c, options));
                    command.OnExecute(() =>
                    {
                        WriteLogo();
                        command.ShowHelp();
                    });
                });

            app.OnExecute(() =>
            {
                WriteLogo();
                app.ShowHelp();
            });

            var result = app.Execute(args);

            if (result != 0)
            {
                return null;
            }

            options.IsHelp = app.IsShowingInformation;

            options.Verbose = verbose.HasValue();
            options.StartupProject = startupProjectOption.Value();
            options.TargetProject = targetProjectOption.Value();
            options.Framework = frameworkOption.HasValue()
                ? NuGetFramework.Parse(frameworkOption.Value())
                : null;
            options.Configuration = configurationOption.Value();
            options.BuildBasePath = buildBasePathOption.Value();
            options.BuildOutputPath = outputOption.Value();
            options.NoBuild = noBuildOption.HasValue();
            options.EnvironmentName = environment.Value();

            return options;
        }

        public string BuildBasePath { get; set; }

        public string Configuration { get; set; }

        public bool NoBuild { get; set; }

        public string BuildOutputPath { get; set; }

        public NuGetFramework Framework { get; set; }

        public string TargetProject { get; set; }

        public string StartupProject { get; set; }

        private static void WriteLogo()
        {
            const string Bold = "\x1b[1m";
            const string Normal = "\x1b[22m";
            const string Magenta = "\x1b[35m";
            const string White = "\x1b[37m";
            const string Default = "\x1b[39m";

            Console.WriteLine();
            Console.WriteLine(@"                     _/\__       ".Insert(21, Bold + White));
            Console.WriteLine(@"               ---==/    \\      ".Insert(20, Bold + White));
            Console.WriteLine(@"         ___  ___   |.    \|\    ".Insert(26, Bold).Insert(21, Normal).Insert(20, Bold + White).Insert(9, Normal + Magenta));
            Console.WriteLine(@"        | __|| __|  |  )   \\\   ".Insert(20, Bold + White).Insert(8, Normal + Magenta));
            Console.WriteLine(@"        | _| | _|   \_/ |  //|\\ ".Insert(20, Bold + White).Insert(8, Normal + Magenta));
            Console.WriteLine(@"        |___||_|       /   \\\/\\".Insert(33, Normal + Default).Insert(23, Bold + White).Insert(8, Normal + Magenta));
            Console.WriteLine();
        }
    }
}
