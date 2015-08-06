// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Configuration
{
    public class RelationshipConfiguration
    {
        public RelationshipConfiguration([NotNull] EntityConfiguration entityConfiguration,
            [NotNull] IForeignKey foreignKey, [NotNull] string dependentEndNavigationPropertyName,
            [NotNull] string principalEndNavigationPropertyName)
        {
            Check.NotNull(entityConfiguration, nameof(entityConfiguration));
            Check.NotNull(foreignKey, nameof(foreignKey));
            Check.NotEmpty(dependentEndNavigationPropertyName, nameof(dependentEndNavigationPropertyName));
            Check.NotEmpty(principalEndNavigationPropertyName, nameof(principalEndNavigationPropertyName));

            EntityConfiguration = entityConfiguration;
            ForeignKey = foreignKey;
            DependentEndNavigationPropertyName = dependentEndNavigationPropertyName;
            PrincipalEndNavigationPropertyName = principalEndNavigationPropertyName;
        }

        public virtual EntityConfiguration EntityConfiguration { get; [param: NotNull] private set; }
        public virtual IForeignKey ForeignKey { get; [param: NotNull] private set; }
        public virtual string DependentEndNavigationPropertyName { get; [param: NotNull] private set; }
        public virtual string PrincipalEndNavigationPropertyName { get; [param: NotNull] private set; }

        public override string ToString()
        {
            string dependentEndLambdaIdentifier ="d"; 
            string principalEndLambdaIdentifier = "p";
            var sb = new StringBuilder();
            sb.Append("Reference(");
            sb.Append(dependentEndLambdaIdentifier);
            sb.Append(" => ");
            sb.Append(dependentEndLambdaIdentifier);
            sb.Append(".");
            sb.Append(DependentEndNavigationPropertyName);
            sb.Append(")");

            if (ForeignKey.IsUnique)
            {
                sb.Append(".InverseReference(");
            }
            else
            {
                sb.Append(".InverseCollection(");
            }
            if (!string.IsNullOrEmpty(PrincipalEndNavigationPropertyName))
            {
                sb.Append(principalEndLambdaIdentifier);
                sb.Append(" => ");
                sb.Append(principalEndLambdaIdentifier);
                sb.Append(".");
                sb.Append(PrincipalEndNavigationPropertyName);
            }
            sb.Append(")");

            sb.Append(".ForeignKey");
            if (ForeignKey.IsUnique)
            {
                // If the relationship is 1:1 need to define to which end
                // the ForeignKey properties belong.
                sb.Append("<");
                sb.Append(EntityConfiguration.EntityType.DisplayName());
                sb.Append(">");
            }

            sb.Append("(d => ");
            sb.Append(new ModelUtilities().GenerateLambdaToKey(ForeignKey.Properties, dependentEndLambdaIdentifier));
            sb.Append(")");

            return sb.ToString();
        }
    }
}
