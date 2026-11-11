<#
.SYNOPSIS
    Generates the binary-compatibility suppressions of one kind of announced break: a public method that left a
    package class for a class go2cs-gen marks [GoPromotedHost(typeof(S))], unchanged in every other respect.

.DESCRIPTION
    1.26.10.4 moved every struct's same-package promoted-method forwarders out of the package class into a class of
    the struct's own (docs/phase4/DESIGN-promoted-forwarder-placement.md). A compiled consumer binds a forwarder by
    its declaring class, so each moved public forwarder is a CP0002 the release must announce, one entry per asset.
    They are too many to write by hand, and a hand-written list would also hide any member that left for another
    reason. This module writes the entries from the two assemblies themselves:

      a member is REMOVED when the baseline's public surface has its documentation id and the current one has not;
      a removed method is a MOVE when the current assembly holds a class marked [go.GoPromotedHost(typeof(S))], S
      being the struct the method's first parameter takes (S, or go.zh<S> for the pointer form), and that class
      declares a method of the same name, signature and attributes.

    Nullable annotations are compared DECODED. The compiler writes one NullableContext on a class, the value most
    of its members share, and repeats the attribute only on a member that differs; so the attributes a method
    itself carries depend on the other members of its class, and a method that moves between classes changes them
    while its annotations stay what they were. Each position of a signature is therefore read as its own Nullable
    attribute or, without one, the context of the nearest enclosing method, class or module, and a position whose
    type takes no annotation is read as none. A method that moved AND changed an annotation is refused.

    Only a move gets an entry. Every other removed member is returned as REFUSED with its reason, and the guard
    (GoApiCompat.psm1) still reads it red: a break nobody announced.

    The surface is read with the Roslyn that PowerShell 7 ships for Add-Type, so the documentation ids are the ones
    the ApiCompat engine prints. Windows PowerShell 5.1 has no Roslyn: Test-GoApiCompatMovesAvailable says so, and
    the generator is a pwsh tool. The guard, not this module, decides a release; it runs under either shell.
#>
#Requires -Version 5.1
Set-StrictMode -Version 2

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

$script:AssetPattern = '^(lib/[^/]+|runtimes/[^/]+/lib/[^/]+)/[^/]+\.dll$'

$script:FinderSource = @'
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Srm = System.Reflection.Metadata;

namespace Go2cs.ApiCompatMoves
{
    public sealed class Move
    {
        public string Target;
        public string Host;
    }

    public sealed class Refusal
    {
        public string Target;
        public string Reason;
    }

    public sealed class Result
    {
        public List<Move> Moves = new List<Move>();
        public List<Refusal> Refused = new List<Refusal>();
    }

    public static class Finder
    {
        private const string HostAttribute = "go.GoPromotedHostAttribute";

        private const string NullableAttribute = "System.Runtime.CompilerServices.NullableAttribute";
        private const string NullableContextAttribute = "System.Runtime.CompilerServices.NullableContextAttribute";

        // Types are written without their nullable annotations: Nullability reads those from the tables.
        private static readonly SymbolDisplayFormat s_type = SymbolDisplayFormat.FullyQualifiedFormat;

        private static readonly Annotated s_annotated = new Annotated();

        // An assembly's metadata tables, beside the symbols Load makes of the same file.
        private sealed class Tables : IDisposable
        {
            private readonly FileStream m_stream;
            private readonly PEReader m_image;

            public readonly Srm.MetadataReader Reader;

            public Tables(string path)
            {
                m_stream = File.OpenRead(path);
                m_image = new PEReader(m_stream);
                Reader = Srm.PEReaderExtensions.GetMetadataReader(m_image);
            }

            public void Dispose()
            {
                m_image.Dispose();
                m_stream.Dispose();
            }
        }

        public static Result Find(string baselinePath, string currentPath)
        {
            IAssemblySymbol baseline = Load(baselinePath), current = Load(currentPath);

            using (Tables baselineTables = new Tables(baselinePath))
            using (Tables currentTables = new Tables(currentPath))
            {
                Dictionary<string, ISymbol> before = Surface(baseline), after = Surface(current);
                Dictionary<string, List<INamedTypeSymbol>> hosts = Hosts(currentTables.Reader, current);
                Result result = new Result();

                foreach (KeyValuePair<string, ISymbol> entry in before.OrderBy(pair => pair.Key, StringComparer.Ordinal))
                {
                    if (after.ContainsKey(entry.Key))
                        continue;

                    // The engine reports a removed type once and none of its members.
                    if (entry.Value.ContainingType != null && !after.ContainsKey(Id(entry.Value.ContainingType)))
                        continue;

                    string host, reason;

                    if (Moved(entry.Value, hosts, baselineTables.Reader, currentTables.Reader, out host, out reason))
                        result.Moves.Add(new Move { Target = entry.Key, Host = host });
                    else
                        result.Refused.Add(new Refusal { Target = entry.Key, Reason = reason });
                }

                return result;
            }
        }

        private static IAssemblySymbol Load(string path)
        {
            MetadataReference reference = MetadataReference.CreateFromImage(File.ReadAllBytes(path));
            CSharpCompilation compilation = CSharpCompilation.Create("moves", references: new[] { reference });
            IAssemblySymbol assembly = compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;

            if (assembly == null)
                throw new InvalidOperationException(path + " is not an assembly");

            return assembly;
        }

        private static string Id(ISymbol symbol)
        {
            return symbol.GetDocumentationCommentId() ?? "";
        }

        private static bool Visible(ISymbol symbol)
        {
            for (ISymbol each = symbol; each != null && !(each is INamespaceSymbol); each = each.ContainingSymbol)
            {
                Accessibility access = each.DeclaredAccessibility;

                if (access != Accessibility.Public && access != Accessibility.Protected && access != Accessibility.ProtectedOrInternal)
                    return false;
            }

            return true;
        }

        private static IEnumerable<INamedTypeSymbol> Types(INamespaceOrTypeSymbol container)
        {
            foreach (ISymbol member in container.GetMembers())
            {
                INamespaceSymbol space = member as INamespaceSymbol;
                INamedTypeSymbol type = member as INamedTypeSymbol;

                if (space != null)
                {
                    foreach (INamedTypeSymbol nested in Types(space))
                        yield return nested;
                }
                else if (type != null)
                {
                    yield return type;

                    foreach (INamedTypeSymbol nested in Types(type))
                        yield return nested;
                }
            }
        }

        // Every type and member visible outside the assembly, by documentation id.
        private static Dictionary<string, ISymbol> Surface(IAssemblySymbol assembly)
        {
            Dictionary<string, ISymbol> surface = new Dictionary<string, ISymbol>(StringComparer.Ordinal);

            foreach (INamedTypeSymbol type in Types(assembly.GlobalNamespace))
            {
                if (!Visible(type))
                    continue;

                surface[Id(type)] = type;

                foreach (ISymbol member in type.GetMembers())
                {
                    if (member is INamedTypeSymbol || member.IsImplicitlyDeclared || !Visible(member))
                        continue;

                    IMethodSymbol method = member as IMethodSymbol;

                    // An accessor is reported through its property or event.
                    if (method != null && method.AssociatedSymbol != null)
                        continue;

                    surface[Id(member)] = member;
                }
            }

            return surface;
        }

        private static string MetadataName(INamedTypeSymbol type)
        {
            string name = type.MetadataName;

            if (type.ContainingType != null)
                return MetadataName(type.ContainingType) + "+" + name;

            string space = Namespace(type.ContainingNamespace);

            return space.Length == 0 ? name : space + "." + name;
        }

        // A namespace as metadata writes it. Not ToDisplayString: that escapes a segment that is a C# keyword
        // (go.@internal.abi), and the name an attribute stores is not escaped.
        private static string Namespace(INamespaceSymbol space)
        {
            if (space == null || space.IsGlobalNamespace)
                return "";

            string outer = Namespace(space.ContainingNamespace);

            return outer.Length == 0 ? space.MetadataName : outer + "." + space.MetadataName;
        }

        // The classes marked [go.GoPromotedHost(typeof(S))], by S. The attribute is read from the metadata tables and
        // not through the compilation: each assembly is loaded alone, as the guard compares it, so neither the
        // attribute's class (go.lib's) nor System.Type resolves, and the compiler decodes no argument of such an
        // attribute. The argument of typeof is stored as the type's name, which is all that is needed here.
        private static Dictionary<string, List<INamedTypeSymbol>> Hosts(Srm.MetadataReader reader, IAssemblySymbol assembly)
        {
            Dictionary<string, List<INamedTypeSymbol>> hosts = new Dictionary<string, List<INamedTypeSymbol>>(StringComparer.Ordinal);

            foreach (Srm.TypeDefinitionHandle handle in reader.TypeDefinitions)
            {
                Srm.TypeDefinition definition = reader.GetTypeDefinition(handle);

                // A marked class is a top-level class.
                if (!definition.GetDeclaringType().IsNil)
                    continue;

                foreach (Srm.CustomAttributeHandle attributeHandle in definition.GetCustomAttributes())
                {
                    Srm.CustomAttribute attribute = reader.GetCustomAttribute(attributeHandle);

                    if (AttributeClass(reader, attribute) != HostAttribute)
                        continue;

                    Srm.BlobReader value = reader.GetBlobReader(attribute.Value);

                    if (value.Length < 3 || value.ReadUInt16() != 1)
                        continue;

                    // A type of another assembly is written with that assembly's name after a comma. A struct and
                    // its class are in one assembly, so such an argument marks nothing.
                    string key = value.ReadSerializedString();

                    if (string.IsNullOrEmpty(key) || key.IndexOf(',') >= 0)
                        continue;

                    INamedTypeSymbol type = assembly.GetTypeByMetadataName(Qualified(reader.GetString(definition.Namespace), reader.GetString(definition.Name)));

                    if (type == null || !type.IsStatic || !Visible(type))
                        continue;

                    List<INamedTypeSymbol> list;

                    if (!hosts.TryGetValue(key, out list))
                        hosts[key] = list = new List<INamedTypeSymbol>();

                    list.Add(type);
                }
            }

            return hosts;
        }

        private static string Qualified(string space, string name)
        {
            return space.Length == 0 ? name : space + "." + name;
        }

        // The class of an attribute, from its constructor: a reference into another assembly, or a definition here.
        private static string AttributeClass(Srm.MetadataReader reader, Srm.CustomAttribute attribute)
        {
            if (attribute.Constructor.Kind == Srm.HandleKind.MemberReference)
            {
                Srm.EntityHandle parent = reader.GetMemberReference((Srm.MemberReferenceHandle)attribute.Constructor).Parent;

                if (parent.Kind == Srm.HandleKind.TypeReference)
                {
                    Srm.TypeReference reference = reader.GetTypeReference((Srm.TypeReferenceHandle)parent);
                    return Qualified(reader.GetString(reference.Namespace), reader.GetString(reference.Name));
                }

                if (parent.Kind != Srm.HandleKind.TypeDefinition)
                    return "";

                Srm.TypeDefinition inAssembly = reader.GetTypeDefinition((Srm.TypeDefinitionHandle)parent);
                return Qualified(reader.GetString(inAssembly.Namespace), reader.GetString(inAssembly.Name));
            }

            if (attribute.Constructor.Kind != Srm.HandleKind.MethodDefinition)
                return "";

            Srm.TypeDefinition declaring = reader.GetTypeDefinition(reader.GetMethodDefinition((Srm.MethodDefinitionHandle)attribute.Constructor).GetDeclaringType());
            return Qualified(reader.GetString(declaring.Namespace), reader.GetString(declaring.Name));
        }

        private static bool Moved(ISymbol removed, Dictionary<string, List<INamedTypeSymbol>> hosts, Srm.MetadataReader baseline,
                                  Srm.MetadataReader current, out string host, out string reason)
        {
            host = null;
            IMethodSymbol method = removed as IMethodSymbol;

            if (method == null || method.MethodKind != MethodKind.Ordinary || !method.IsStatic || method.Parameters.Length == 0)
            {
                reason = "removed, and not a static method with a receiver parameter";
                return false;
            }

            INamedTypeSymbol receiver = method.Parameters[0].Type as INamedTypeSymbol;

            // The pointer form takes the struct's box.
            if (receiver != null && receiver.Arity == 1 && receiver.MetadataName == "\u0436`1" && receiver.ContainingType == null &&
                Namespace(receiver.ContainingNamespace) == "go")
            {
                receiver = receiver.TypeArguments[0] as INamedTypeSymbol;
            }

            if (receiver == null)
            {
                reason = "removed, and its first parameter is not a named type";
                return false;
            }

            string key = MetadataName(receiver.OriginalDefinition);
            List<INamedTypeSymbol> candidates;

            if (!hosts.TryGetValue(key, out candidates))
            {
                reason = "removed, and no class is marked GoPromotedHost(typeof(" + key + "))";
                return false;
            }

            string signature = Signature(baseline, method);
            List<string> declared = new List<string>();

            foreach (INamedTypeSymbol candidate in candidates)
            {
                foreach (IMethodSymbol other in candidate.GetMembers(method.Name).OfType<IMethodSymbol>())
                {
                    string found = Signature(current, other);

                    if (found == signature)
                    {
                        host = MetadataName(candidate);
                        reason = null;
                        return true;
                    }

                    declared.Add(found);
                }
            }

            reason = "removed, and " + string.Join(", ", candidates.Select(MetadataName)) + " declares no " + method.Name +
                     " of the same signature and attributes (" + signature + ")" +
                     (declared.Count == 0 ? "" : "; declared there: " + string.Join(" | ", declared));
            return false;
        }

        // The two attributes that ENCODE nullable annotations. Where the compiler writes them depends on the other
        // members of the class, so they are not compared as attributes: Nullability compares what they say.
        private static bool EncodesNullability(AttributeData attribute)
        {
            INamedTypeSymbol type = attribute.AttributeClass;

            return type != null && Namespace(type.ContainingNamespace) == "System.Runtime.CompilerServices" &&
                   (type.MetadataName == "NullableAttribute" || type.MetadataName == "NullableContextAttribute");
        }

        private static string Attributes(IEnumerable<AttributeData> attributes)
        {
            return string.Join(";", attributes.Where(attribute => !EncodesNullability(attribute)).Select(attribute => attribute.ToString())
                .OrderBy(text => text, StringComparer.Ordinal));
        }

        // Whether a type in a signature takes a nullable annotation: every type but a primitive, a non-generic value
        // type and a System.Nullable<T> of one. A signature says class or value type itself, so no type has to
        // resolve. 2 is System.Nullable<T> before its argument is known.
        private sealed class Annotated : Srm.ISignatureTypeProvider<int, object>
        {
            private const byte ValueType = 0x11;

            public int GetPrimitiveType(Srm.PrimitiveTypeCode code)
            {
                return code == Srm.PrimitiveTypeCode.String || code == Srm.PrimitiveTypeCode.Object ? 1 : 0;
            }

            public int GetTypeFromDefinition(Srm.MetadataReader reader, Srm.TypeDefinitionHandle handle, byte rawTypeKind)
            {
                return rawTypeKind == ValueType ? 0 : 1;
            }

            public int GetTypeFromReference(Srm.MetadataReader reader, Srm.TypeReferenceHandle handle, byte rawTypeKind)
            {
                if (rawTypeKind != ValueType)
                    return 1;

                Srm.TypeReference reference = reader.GetTypeReference(handle);

                return reader.GetString(reference.Name) == "Nullable`1" && reader.GetString(reference.Namespace) == "System" ? 2 : 0;
            }

            public int GetTypeFromSpecification(Srm.MetadataReader reader, object context, Srm.TypeSpecificationHandle handle, byte rawTypeKind)
            {
                return reader.GetTypeSpecification(handle).DecodeSignature(this, context);
            }

            public int GetGenericInstantiation(int genericType, ImmutableArray<int> typeArguments)
            {
                return genericType == 2 ? Math.Min(typeArguments[0], 1) : 1;
            }

            public int GetSZArrayType(int elementType) { return 1; }

            public int GetArrayType(int elementType, Srm.ArrayShape shape) { return 1; }

            public int GetByReferenceType(int elementType) { return elementType; }

            public int GetPointerType(int elementType) { return elementType; }

            public int GetPinnedType(int elementType) { return elementType; }

            public int GetModifiedType(int modifier, int unmodifiedType, bool isRequired) { return unmodifiedType; }

            public int GetGenericMethodParameter(object context, int index) { return 1; }

            public int GetGenericTypeParameter(object context, int index) { return 1; }

            public int GetFunctionPointerType(Srm.MethodSignature<int> signature) { return 1; }
        }

        // What one nullable attribute among these says, or null without one: a byte for every annotated type of the
        // position, or a counted list of them, one each.
        private static string NullableBytes(Srm.MetadataReader reader, Srm.CustomAttributeHandleCollection attributes, string attributeClass)
        {
            foreach (Srm.CustomAttributeHandle handle in attributes)
            {
                Srm.CustomAttribute attribute = reader.GetCustomAttribute(handle);

                if (AttributeClass(reader, attribute) != attributeClass)
                    continue;

                // The prolog (2 bytes), the argument, the count of named arguments (2 bytes).
                byte[] blob = reader.GetBlobBytes(attribute.Value);

                if (blob.Length == 5)
                    return blob[2].ToString();

                if (blob.Length >= 8)
                    return "[" + string.Join(",", blob.Skip(6).Take(blob.Length - 8).Select(each => each.ToString())) + "]";

                return "unread";
            }

            return null;
        }

        // The context a method's positions take without an attribute of their own: the method's, else the nearest
        // enclosing class's, else the module's, else oblivious.
        private static string NullableContext(Srm.MetadataReader reader, Srm.MethodDefinition method)
        {
            string context = NullableBytes(reader, method.GetCustomAttributes(), NullableContextAttribute);
            Srm.TypeDefinitionHandle handle = method.GetDeclaringType();

            while (context == null && !handle.IsNil)
            {
                Srm.TypeDefinition type = reader.GetTypeDefinition(handle);

                context = NullableBytes(reader, type.GetCustomAttributes(), NullableContextAttribute);
                handle = type.GetDeclaringType();
            }

            if (context == null)
                context = NullableBytes(reader, reader.GetCustomAttributes(Srm.EntityHandle.ModuleDefinition), NullableContextAttribute);

            return context ?? "0";
        }

        // The nullable annotations of a method, decoded: the return, each parameter, then each type parameter with
        // its constraints. Equal for one method declared in two classes, whatever each class's own context is.
        private static string Nullability(Srm.MetadataReader reader, IMethodSymbol method)
        {
            Srm.EntityHandle handle = Srm.Ecma335.MetadataTokens.EntityHandle(method.MetadataToken);

            if (handle.IsNil || handle.Kind != Srm.HandleKind.MethodDefinition)
                return "unread";

            Srm.MethodDefinition definition = reader.GetMethodDefinition((Srm.MethodDefinitionHandle)handle);
            string context = NullableContext(reader, definition);
            Srm.MethodSignature<int> shape = definition.DecodeSignature(s_annotated, null);
            string[] positions = new string[shape.ParameterTypes.Length + 1];

            foreach (Srm.ParameterHandle parameterHandle in definition.GetParameters())
            {
                Srm.Parameter parameter = reader.GetParameter(parameterHandle);

                if (parameter.SequenceNumber < positions.Length)
                    positions[parameter.SequenceNumber] = NullableBytes(reader, parameter.GetCustomAttributes(), NullableAttribute);
            }

            for (int i = 0; i < positions.Length; i++)
            {
                int annotated = i == 0 ? shape.ReturnType : shape.ParameterTypes[i - 1];

                positions[i] = annotated == 0 ? "-" : positions[i] ?? context;
            }

            List<string> typeParameters = new List<string>();

            foreach (Srm.GenericParameterHandle parameterHandle in definition.GetGenericParameters())
            {
                Srm.GenericParameter parameter = reader.GetGenericParameter(parameterHandle);
                bool valueType = (parameter.Attributes & System.Reflection.GenericParameterAttributes.NotNullableValueTypeConstraint) != 0;
                string text = valueType ? "-" : NullableBytes(reader, parameter.GetCustomAttributes(), NullableAttribute) ?? context;

                foreach (Srm.GenericParameterConstraintHandle constraintHandle in parameter.GetConstraints())
                {
                    Srm.GenericParameterConstraint constraint = reader.GetGenericParameterConstraint(constraintHandle);

                    text += ":" + (NullableBytes(reader, constraint.GetCustomAttributes(), NullableAttribute) ?? context);
                }

                typeParameters.Add(text);
            }

            return string.Join(" ", positions) + " <" + string.Join(",", typeParameters) + ">";
        }

        // Everything a caller compiled against the method depends on, the declaring type left out.
        private static string Signature(Srm.MetadataReader reader, IMethodSymbol method)
        {
            IEnumerable<string> parameters = method.Parameters.Select(parameter =>
                "[" + Attributes(parameter.GetAttributes()) + "]" + parameter.RefKind + " " + parameter.Type.ToDisplayString(s_type) + " " + parameter.Name +
                (parameter.IsParams ? " params" : "") + (parameter.HasExplicitDefaultValue ? " = " + (parameter.ExplicitDefaultValue ?? "null") : ""));

            IEnumerable<string> typeParameters = method.TypeParameters.Select(parameter =>
                parameter.Name + ":" + string.Join("&", parameter.ConstraintTypes.Select(type => type.ToDisplayString(s_type))) +
                (parameter.HasReferenceTypeConstraint ? "&class" : "") + (parameter.HasValueTypeConstraint ? "&struct" : "") +
                (parameter.HasUnmanagedTypeConstraint ? "&unmanaged" : "") + (parameter.HasNotNullConstraint ? "&notnull" : "") +
                (parameter.HasConstructorConstraint ? "&new()" : ""));

            return "[" + Attributes(method.GetAttributes()) + "][return: " + Attributes(method.GetReturnTypeAttributes()) + "] " +
                   method.DeclaredAccessibility + (method.IsExtensionMethod ? " extension " : " static ") + method.RefKind + " " +
                   method.ReturnType.ToDisplayString(s_type) + " " + method.Name + "<" + string.Join(",", typeParameters) + ">(" + string.Join(", ", parameters) +
                   ") nullable " + Nullability(reader, method);
        }
    }
}
'@

# Whether this PowerShell can read an assembly's surface: PowerShell 7 ships Roslyn for Add-Type, Windows PowerShell
# 5.1 does not.
function Test-GoApiCompatMovesAvailable {
    [bool]('Microsoft.CodeAnalysis.CSharp.CSharpCompilation' -as [type])
}

function Initialize-GoApiCompatMoves {
    if ('Go2cs.ApiCompatMoves.Finder' -as [type]) { return }
    if (-not (Test-GoApiCompatMovesAvailable)) {
        throw "This PowerShell ($($PSVersionTable.PSVersion)) ships no Roslyn (Microsoft.CodeAnalysis.CSharp): generate suppressions with pwsh, PowerShell 7"
    }
    # Roslyn by path; the framework by simple name, which Add-Type resolves in PowerShell's own reference
    # assemblies. Roslyn is built against an older System.Runtime than those, which the compiler reports as
    # warning CS1701 on every reference, so warnings are not errors here.
    $roslyn = @([Microsoft.CodeAnalysis.Compilation].Assembly.Location, [Microsoft.CodeAnalysis.CSharp.CSharpCompilation].Assembly.Location)
    $references = $roslyn + @('System.Runtime', 'System.Collections', 'System.Collections.Immutable', 'System.Linq', 'System.Reflection.Metadata', 'netstandard')
    Add-Type -TypeDefinition $script:FinderSource -ReferencedAssemblies $references -IgnoreWarnings -WarningAction SilentlyContinue
}

# The moves and the refused removals between two builds of one assembly.
function Get-GoApiCompatMoves([Parameter(Mandatory)][string]$Baseline, [Parameter(Mandatory)][string]$Current) {
    Initialize-GoApiCompatMoves
    [Go2cs.ApiCompatMoves.Finder]::Find((Resolve-Path -LiteralPath $Baseline).ProviderPath, (Resolve-Path -LiteralPath $Current).ProviderPath)
}

function Get-MovesAssets([string]$Package) {
    $zip = [System.IO.Compression.ZipFile]::OpenRead($Package)
    try { @($zip.Entries | ForEach-Object { $_.FullName -replace '\\', '/' } | Where-Object { $_ -match $script:AssetPattern } | Sort-Object) }
    finally { $zip.Dispose() }
}

function Expand-MovesAsset([string]$Package, [string]$Asset, [string]$Path) {
    $zip = [System.IO.Compression.ZipFile]::OpenRead($Package)
    try {
        $entry = @($zip.Entries | Where-Object { ($_.FullName -replace '\\', '/') -ceq $Asset })[0]
        New-Item -ItemType Directory -Force (Split-Path -Parent $Path) | Out-Null
        [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $Path, $true)
    }
    finally { $zip.Dispose() }
}

<#
    The moves between a baseline package and the current one, asset by asset, as the guard compares them. Returns:
      Entries  one per moved member per asset: Asset, Target (its documentation id), Host (the class that has it now)
      Refused  one per other removed member per asset: Asset, Target, Reason
    An asset only one of the packages carries is not read: the guard reports a missing one itself.
#>
function Get-GoApiCompatPackageMoves {
    param(
        [Parameter(Mandatory)][string]$Baseline,
        [Parameter(Mandatory)][string]$Current,
        [Parameter(Mandatory)][string]$WorkDir
    )

    $entries = New-Object System.Collections.Generic.List[object]
    $refused = New-Object System.Collections.Generic.List[object]
    $currentAssets = @(Get-MovesAssets $Current)
    $work = Join-Path $WorkDir ('moves-' + [guid]::NewGuid().ToString('N'))

    try {
        foreach ($asset in @(Get-MovesAssets $Baseline)) {
            if ($currentAssets -notcontains $asset) { continue }
            $left = Join-Path (Join-Path $work 'left') $asset
            $right = Join-Path (Join-Path $work 'right') $asset
            Expand-MovesAsset $Baseline $asset $left
            Expand-MovesAsset $Current $asset $right
            $found = Get-GoApiCompatMoves -Baseline $left -Current $right
            foreach ($move in $found.Moves) { $entries.Add([pscustomobject]@{ Asset = $asset; Target = $move.Target; Host = $move.Host }) }
            foreach ($refusal in $found.Refused) { $refused.Add([pscustomobject]@{ Asset = $asset; Target = $refusal.Target; Reason = $refusal.Reason }) }
        }
    }
    finally { if (Test-Path -LiteralPath $work) { Remove-Item -LiteralPath $work -Recurse -Force } }

    [pscustomobject]@{ Entries = $entries.ToArray(); Refused = $refused.ToArray() }
}

<#
    Writes a package's CompatibilitySuppressions.xml: the guard's assembly-mode entries (Left and Right name the asset,
    no IsBaselineSuppression), sorted by asset and then by target, under a comment that says what announced them.
    UTF-8 without a byte-order mark and LF line ends, as the files before it. $Comment is the header's text; it must
    not hold two hyphens in a row.
#>
function Write-GoApiCompatMoveSuppressions {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][object[]]$Entries,
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Comment
    )

    if ($Comment.Contains('--')) { throw 'The comment holds "--", which an XML comment cannot' }
    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add('<?xml version="1.0" encoding="utf-8"?>')
    $lines.Add('<!--')
    foreach ($line in ($Comment -split "`r?`n")) { $lines.Add(("  " + $line).TrimEnd()) }
    $lines.Add('-->')
    $lines.Add('<Suppressions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">')
    $sorted = @($Entries | Sort-Object -Property @{ Expression = { $_.Asset } }, @{ Expression = { $_.Target } } -CaseSensitive -Culture '')
    foreach ($entry in $sorted) {
        $lines.Add('  <Suppression>')
        $lines.Add('    <DiagnosticId>CP0002</DiagnosticId>')
        $lines.Add("    <Target>$([System.Security.SecurityElement]::Escape($entry.Target))</Target>")
        $lines.Add("    <Left>$($entry.Asset)</Left>")
        $lines.Add("    <Right>$($entry.Asset)</Right>")
        $lines.Add('  </Suppression>')
    }
    $lines.Add('</Suppressions>')
    [System.IO.File]::WriteAllText($Path, (($lines -join "`n") + "`n"), (New-Object System.Text.UTF8Encoding $false))
}

Export-ModuleMember -Function Test-GoApiCompatMovesAvailable, Get-GoApiCompatMoves, Get-GoApiCompatPackageMoves, Write-GoApiCompatMoveSuppressions
