// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Mustache;

namespace Microsoft.Data.Entity.Relational.Design.Templating
{
    public class MustacheTemplating : ITemplating
    {
        public virtual TemplateResult RunTemplate(string template, dynamic model)
        {
            var compiler = new FormatCompiler
                {
                    RemoveNewLines = false
                };
            try
            {
                var generator = compiler.Compile(template);
                var result = generator.Render(model);
                return new TemplateResult
                {
                    GeneratedText = result,
                    ProcessingException = null
                };
            }
            catch (FormatException e)
            {
                return new TemplateResult
                {
                    GeneratedText = string.Empty,
                    ProcessingException = new TemplateProcessingException(new[] { e.Message })
                };
            }
        }
    }
}
