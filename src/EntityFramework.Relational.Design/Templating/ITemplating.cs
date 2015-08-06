// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational.Design.Templating
{
    public interface ITemplating
    {
        TemplateResult RunTemplate([NotNull] string template, [NotNull] dynamic model);
    }
}
