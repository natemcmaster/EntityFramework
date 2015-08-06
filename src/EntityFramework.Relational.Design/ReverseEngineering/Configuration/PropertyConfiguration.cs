// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Configuration
{
    public class PropertyConfiguration
    {
        public PropertyConfiguration(
            [NotNull] EntityConfiguration entityConfiguration, [NotNull] IProperty property)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));
            Check.NotNull(property, nameof(property));

            EntityConfiguration = entityConfiguration;
            Property = property;
        }

        public virtual EntityConfiguration EntityConfiguration { get; [param: NotNull] private set; }
        public virtual IProperty Property { get; [param: NotNull] private set; }
        public virtual Dictionary<string, List<string>> FacetConfigurations { get; } = new Dictionary<string, List<string>>();

        public virtual void AddFacetConfiguration([NotNull] FacetConfiguration facetConfiguration)
        {
            Check.NotNull(facetConfiguration, nameof(facetConfiguration));

            var @for = facetConfiguration.For ?? string.Empty;
            List<string> listOfFacetMethodBodies;
            if (!FacetConfigurations.TryGetValue(@for, out listOfFacetMethodBodies))
            {
                listOfFacetMethodBodies = new List<string>();
                FacetConfigurations.Add(@for, listOfFacetMethodBodies);
            }
            listOfFacetMethodBodies.Add(facetConfiguration.MethodBody);
        }

        //definitely not hte right place for this
        public override string ToString()
        {
            var propertyLambdaIdentifier = "property";
            var indent = "    ";
            var lines = new List<string>();
            var facetCount = FacetConfigurations.Values.SelectMany(list => list).Count();
            foreach (var keyValuePair in FacetConfigurations)
            {
                var forMethod = keyValuePair.Key;
                var methodBodyList = keyValuePair.Value;
                if (string.IsNullOrEmpty(forMethod))
                {
                    foreach (var methodBody in methodBodyList)
                    {
                        lines.Add("." + methodBody);
                    }
                }
                else
                {
                    if (methodBodyList.Count() == 1)
                    {
                        lines.Add("." + forMethod + "()." + methodBodyList.First());
                    }
                    else
                    {
                        lines.Add("." + forMethod + "(" + propertyLambdaIdentifier + " =>");
                        lines.Add("{");
                        foreach (var methodBody in methodBodyList)
                        {
                            lines.Add(indent + propertyLambdaIdentifier + "." + methodBody + ";");
                        }
                        lines.Add("})");
                    }
                }
            }
            return string.Join(Environment.NewLine,lines);
        }
    }
}
