using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TypeExtensions = go.golib.TypeExtensions;

namespace GolibTests;

// The extension-method scan finds the classes that declare extension methods in an assembly's METADATA TABLES
// (TypeExtensions.ExtensionHostTypes) instead of loading every type with Assembly.GetTypes(), which stays the
// fallback for an assembly without raw metadata. The two must yield the same methods in the same order:
// GetExtensionMethod breaks ties by that order. docs/phase4/DESIGN-extension-scan-metadata.md.
[TestClass]
public class ExtensionScanMetadataTests
{
    [TestMethod]
    public void TheMetadataTablesYieldExactlyTheMethodsGetTypesYields()
    {
        int assemblies = 0, methods = 0;

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.IsDynamic)
                continue;

            Type[] types;

            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException)
            {
                continue; // a test-host assembly with an unresolvable dependency: GetTypes() has no answer to compare
            }

            IReadOnlyList<Type>? hosts = TypeExtensions.ExtensionHostTypes(assembly);
            Assert.IsNotNull(hosts, $"{assembly.GetName().Name}: a JIT-loaded assembly must expose its raw metadata");

            MethodInfo[] viaTypes = [.. types.SelectMany(TypeExtensions.DeclaredExtensionMethods)];
            MethodInfo[] viaMetadata = [.. hosts.SelectMany(TypeExtensions.DeclaredExtensionMethods)];

            CollectionAssert.AreEqual(viaTypes, viaMetadata,
                $"{assembly.GetName().Name}: the metadata tables yielded {viaMetadata.Length} extension methods where GetTypes() yields {viaTypes.Length}");

            assemblies++;
            methods += viaTypes.Length;
        }

        // Control: the comparison must have covered converted packages and the framework, not nothing.
        Assert.IsTrue(assemblies > 20 && methods > 1000, $"control: compared {assemblies} assemblies and {methods} methods");
    }
}
