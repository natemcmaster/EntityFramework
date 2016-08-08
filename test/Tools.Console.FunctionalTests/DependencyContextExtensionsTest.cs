#if NETCOREAPP1_0

using Microsoft.Extensions.DependencyModel;
using System.IO;
using System.Linq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tools.FunctionalTests
{
    public class DependencyContextExtensionsTest
    {
        [Fact]
        public void FindsManagedAssemblies()
        {
            var depsJson = @"
{
  ""runtimeTarget"": {
    ""name"": "".NETCoreApp,Version=v1.0""
  },
  ""compilationOptions"": {},
  ""targets"": {
    "".NETCoreApp,Version=v1.0"": {
      ""Assembly.ManagedOnly/1.0.0"": {
        ""runtime"": {
          ""lib/netcoreapp1.0/Assembly.ManagedOnly.dll"": {}
        }
      },
      ""Assembly.RuntimeMananged/1.0.0"": {
        ""runtimeTargets"": {
          ""runtimes/unix/lib/netstandard1.3/Assembly.RuntimeManaged.dll"": {
            ""rid"": ""unix"",
            ""assetType"": ""runtime""
          },
          ""runtimes/win/lib/netstandard1.3/Assembly.RuntimeManaged.dll"": {
            ""rid"": ""win"",
            ""assetType"": ""runtime""
          }
        }
      },
      ""Package.Native/1.0.0"": {
        ""runtimeTargets"": {
          ""runtimes/linux-x86/native/libnative.so"": {
            ""rid"": ""linux-x86"",
            ""assetType"": ""native""
          },
          ""runtimes/win-x64/native/native.dll"": {
            ""rid"": ""win-x64"",
            ""assetType"": ""native""
          },
          ""runtimes/win7-x64/native/native.dll"": {
            ""rid"": ""win7-x64"",
            ""assetType"": ""native""
          }
        }
      }
    }
  },
  ""libraries"": {
    ""Assembly.ManagedOnly/1.0.0"": {
      ""type"": ""package"",
      ""serviceable"": false,
      ""sha512"": """"
    },
    ""Assembly.RuntimeMananged/1.0.0"": {
      ""type"": ""package"",
      ""serviceable"": false,
      ""sha512"": """"
    },
     ""Package.Native/1.0.0"": {
      ""type"": ""package"",
      ""serviceable"": false,
      ""sha512"": """"
    }
  }
}
";
            var depContext = GetContext(depsJson);

            var runtimeGraph = new RuntimeFallbacks("win7-x64", "win-x64", "win7", "win", "any", "base");

            var managedAssemblies = depContext.ResolveRuntimeAssemblies(runtimeGraph)
                .OrderBy(a => a.Name.Name)
                .ToList();
            Assert.Collection(managedAssemblies,
                assembly =>
                {
                    Assert.Equal(assembly.Name.Name, "Assembly.ManagedOnly");
                    Assert.Equal(assembly.Path, "lib/netcoreapp1.0/Assembly.ManagedOnly.dll");
                },
                assembly =>
                {
                    Assert.Equal(assembly.Name.Name, "Assembly.RuntimeManaged");
                    Assert.Equal(assembly.Path, "runtimes/win/lib/netstandard1.3/Assembly.RuntimeManaged.dll");
                });
        }

        private DependencyContext GetContext(string json)
        {
            var reader = new DependencyContextJsonReader();
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(json);
                writer.Flush();
                stream.Position = 0;
                return reader.Read(stream);
            }
        }
    }
}
#endif