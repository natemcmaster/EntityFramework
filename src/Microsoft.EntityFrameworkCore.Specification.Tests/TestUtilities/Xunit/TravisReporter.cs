// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit
{
    public class TravisReporter : IRunnerReporter
    {
        public IMessageSink CreateMessageHandler(IRunnerLogger logger)
            => new TravisMessageHandler(logger);

        public string Description { get; } = "forces Travis CI reporter mode";

        public bool IsEnvironmentallyEnabled { get; }
            = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TRAVIS"))
              && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISABLE_TRAVIS_REPORTER"));

        public string RunnerSwitch { get; } = "travis";
    }
}
