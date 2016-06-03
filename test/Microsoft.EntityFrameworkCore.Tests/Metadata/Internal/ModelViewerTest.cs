using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Tests
{
    public class ModelPrinterTest
    {
        private readonly ITestOutputHelper _output;
        public ModelPrinterTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void GeneratesModel()
        {
            var modelBuilder = new ModelBuilder(new CoreConventionSetBuilder().CreateConventionSet());
            modelBuilder.Entity<Poco>(e =>
                {
                    e.HasChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);
                    e.HasIndex(t => new { t.Name });
                });
            var model = modelBuilder.Model;

            var s = new ModelPrinter();
            s.VisitModel(model);
            _output.WriteLine(s.ToString());
            Assert.True(false);
        }

        public class Poco
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int ParentId { get; set; }
            public string Type { get; set; }
            public Parent Parent { get; set; }
        }
        public class DerivedPoco : Poco
        {
            public int Height { get; set; }
        }

        public class Parent
        {
            public int Id { get; set; }
        }
    }

    public class ModelPrinter
    {
        private readonly IndentedStringBuilder sb;

        public ModelPrinter()
        {
            sb = new IndentedStringBuilder();
        }

        private void VisitAnnotation(IAnnotation a)
        {
            sb
                .Append(a.Name)
                .AppendLine(": ");
            using (sb.Indent())
            {
                sb.Append("Value: ").AppendLine(a.Value);
                if (a is ConventionalAnnotation)
                {
                    ShowSource((a as ConventionalAnnotation).GetConfigurationSource());
                }
            }
        }

        private void VisitEntityType(IEntityType t)
        {
            sb.AppendLine(t.DisplayName());
            using (sb.Indent())
            {
                ShowProperties(t, new[] { nameof(IEntityType.Model) });

                if (t is EntityType)
                {
                    ShowSource((t as EntityType).GetConfigurationSource());
                }

                sb.AppendLine()
                   .Append("Keys (").Append(t.GetDeclaredKeys().Count()).AppendLine("):");

                using (sb.Indent())
                {
                    var count = 0;
                    foreach (var k in t.GetDeclaredKeys())
                    {
                        sb.Append(count++).AppendLine(":");
                        using (sb.Indent())
                        {
                            VisitKey(k);
                        }
                    }
                }

                sb.AppendLine()
                    .Append("Properties (").Append(t.GetDeclaredProperties().Count()).AppendLine("):");
                using (sb.Indent())
                {
                    t.GetDeclaredProperties().ForEach(p => VisitProperty(p));
                }

                sb.AppendLine()
                  .Append("Indexes (").Append(t.GetDeclaredIndexes().Count()).AppendLine("):");

                using (sb.Indent())
                {
                    var count = 0;
                    foreach (var i in t.GetDeclaredIndexes())
                    {
                        sb.Append(count++).AppendLine(":");
                        using (sb.Indent())
                        {
                            VisitIndex(i);
                        }
                    }
                }

                sb.AppendLine()
                   .Append("Navigations (").Append(t.GetDeclaredNavigations().Count()).AppendLine("):");
                using (sb.Indent())
                {
                    t.GetDeclaredNavigations().ForEach(p => VisitNavigation(p));
                }

                sb.AppendLine()
                    .Append("Annotations (").Append(t.GetAnnotations().Count()).AppendLine("):");
                using (sb.Indent())
                {
                    t.GetAnnotations().ForEach(a => VisitAnnotation(a));
                }
            }
            sb.AppendLine();
        }

        private void VisitIndex(IIndex i)
        {
            sb.Append("Properties: {");
            sb.Append(string.Join(", ", i.Properties.Select(p => p.Name)));
            sb.AppendLine("}");
            PrintProperties(i, nameof(IIndex.IsUnique));
        }

        private void VisitKey(IKey k)
        {
            sb.Append("Properties: {");
            sb.Append(string.Join(", ", k.Properties.Select(p => p.Name)));
            sb.AppendLine("}");
            sb.Append("IsPrimaryKey: ").AppendLine(k.IsPrimaryKey());
        }

        private void VisitProperty(IProperty p)
        {
            sb.AppendLine(p.Name);
            using (sb.Indent())
            {
                if (p is Property)
                {
                    ShowSource((p as Property).GetConfigurationSource());
                }
                ShowProperties(p);

                sb.Append("Annotations (").Append(p.GetAnnotations().Count()).AppendLine("):");
                using (sb.Indent())
                {
                    p.GetAnnotations().ForEach(a => VisitAnnotation(a));
                }
            }
        }

        private void VisitNavigation(INavigation n)
        {
            sb.AppendLine(n.Name);
            using (sb.Indent())
            {
                sb.Append("DeclaringEntityType: ").AppendLine(n.DeclaringEntityType?.DisplayName() ?? "null");
                ShowProperties(n);
            }
        }

        private void ShowSource(ConfigurationSource source)
        {
            sb
                .Append("Source: ")
                .AppendLine(source);
        }

        private void ShowProperties<T>(T instance, string[] except = null)
        {
            foreach (var p in typeof(T)
                .GetTypeInfo()
                .DeclaredProperties
                .OrderBy(p => p.Name)
                .Where(p => except?.Contains(p.Name) != true))
            {
                sb.Append(p.Name).Append(": ").AppendLine(p.GetValue(instance) ?? "null");
            }
        }

        private void PrintProperties<T>(T instance, params string[] propNames)
        {
            var t = typeof(T).GetTypeInfo();
            foreach (var p in propNames?.Select(n => t.GetDeclaredProperty(n)))
            {
                sb.Append(p.Name).Append(": ").AppendLine(p.GetValue(instance) ?? "null");
            }
        }

        public void VisitModel(IModel model)
        {
            sb.Append("Annotations (").Append(model.GetAnnotations().Count()).AppendLine("):");

            using (sb.Indent())
            {
                model.GetAnnotations().OrderBy(a => a.Name).ForEach(a => VisitAnnotation(a));
            }

            sb.Append("Types (").Append(model.GetEntityTypes().Count()).AppendLine("):");
            using (sb.Indent())
            {


                model.GetEntityTypes().OrderBy(t => t.DisplayName()).ForEach(t => VisitEntityType(t));
            }
        }

        public override string ToString()
            => sb.ToString();
    }
}